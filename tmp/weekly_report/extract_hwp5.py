from __future__ import annotations

import argparse
import struct
import zlib
from dataclasses import dataclass
from pathlib import Path


FREESECT = 0xFFFFFFFF
ENDOFCHAIN = 0xFFFFFFFE


@dataclass
class DirEntry:
    name: str
    entry_type: int
    start_sector: int
    size: int


class CompoundFile:
    def __init__(self, path: Path) -> None:
        self.data = path.read_bytes()
        if self.data[:8] != bytes.fromhex("D0CF11E0A1B11AE1"):
            raise ValueError("Not an OLE compound file")

        self.major_version = struct.unpack_from("<H", self.data, 0x1A)[0]
        self.sector_size = 1 << struct.unpack_from("<H", self.data, 0x1E)[0]
        self.mini_sector_size = 1 << struct.unpack_from("<H", self.data, 0x20)[0]
        self.num_fat_sectors = struct.unpack_from("<I", self.data, 0x2C)[0]
        self.first_dir_sector = struct.unpack_from("<I", self.data, 0x30)[0]
        self.mini_cutoff = struct.unpack_from("<I", self.data, 0x38)[0]
        self.first_mini_fat_sector = struct.unpack_from("<I", self.data, 0x3C)[0]
        self.num_mini_fat_sectors = struct.unpack_from("<I", self.data, 0x40)[0]
        self.first_difat_sector = struct.unpack_from("<I", self.data, 0x44)[0]
        self.num_difat_sectors = struct.unpack_from("<I", self.data, 0x48)[0]

        difat = list(struct.unpack_from("<109I", self.data, 0x4C))
        sid = self.first_difat_sector
        for _ in range(self.num_difat_sectors):
            if sid in (FREESECT, ENDOFCHAIN):
                break
            sector = self._sector(sid)
            count = self.sector_size // 4 - 1
            difat.extend(struct.unpack_from(f"<{count}I", sector, 0))
            sid = struct.unpack_from("<I", sector, count * 4)[0]
        fat_sector_ids = [x for x in difat if x not in (FREESECT, ENDOFCHAIN)][: self.num_fat_sectors]

        self.fat: list[int] = []
        for fat_sid in fat_sector_ids:
            self.fat.extend(struct.unpack(f"<{self.sector_size // 4}I", self._sector(fat_sid)))

        directory = self._read_chain(self.first_dir_sector, self.fat)
        self.entries: list[DirEntry] = []
        for offset in range(0, len(directory), 128):
            raw = directory[offset : offset + 128]
            if len(raw) < 128:
                break
            name_len = struct.unpack_from("<H", raw, 64)[0]
            if name_len < 2:
                name = ""
            else:
                name = raw[: name_len - 2].decode("utf-16le", errors="replace")
            entry_type = raw[66]
            start_sector = struct.unpack_from("<I", raw, 116)[0]
            size = struct.unpack_from("<Q", raw, 120)[0]
            if self.major_version == 3:
                size &= 0xFFFFFFFF
            self.entries.append(DirEntry(name, entry_type, start_sector, size))

        root = next(entry for entry in self.entries if entry.entry_type == 5)
        self.mini_stream = self._read_chain(root.start_sector, self.fat, root.size)
        if self.num_mini_fat_sectors and self.first_mini_fat_sector not in (FREESECT, ENDOFCHAIN):
            mini_fat_raw = self._read_chain(
                self.first_mini_fat_sector,
                self.fat,
                self.num_mini_fat_sectors * self.sector_size,
            )
            self.mini_fat = list(struct.unpack(f"<{len(mini_fat_raw) // 4}I", mini_fat_raw))
        else:
            self.mini_fat = []

    def _sector(self, sid: int) -> bytes:
        start = (sid + 1) * self.sector_size
        return self.data[start : start + self.sector_size]

    def _read_chain(self, start_sid: int, fat: list[int], size: int | None = None) -> bytes:
        if start_sid in (FREESECT, ENDOFCHAIN):
            return b""
        chunks: list[bytes] = []
        sid = start_sid
        visited: set[int] = set()
        while sid not in (FREESECT, ENDOFCHAIN):
            if sid in visited or sid >= len(fat):
                raise ValueError(f"Invalid sector chain at {sid}")
            visited.add(sid)
            chunks.append(self._sector(sid))
            sid = fat[sid]
        result = b"".join(chunks)
        return result if size is None else result[:size]

    def _read_mini_chain(self, start_sid: int, size: int) -> bytes:
        if start_sid in (FREESECT, ENDOFCHAIN):
            return b""
        chunks: list[bytes] = []
        sid = start_sid
        visited: set[int] = set()
        while sid not in (FREESECT, ENDOFCHAIN) and len(b"".join(chunks)) < size:
            if sid in visited or sid >= len(self.mini_fat):
                raise ValueError(f"Invalid mini-sector chain at {sid}")
            visited.add(sid)
            start = sid * self.mini_sector_size
            chunks.append(self.mini_stream[start : start + self.mini_sector_size])
            sid = self.mini_fat[sid]
        return b"".join(chunks)[:size]

    def stream(self, name: str) -> bytes:
        matches = [entry for entry in self.entries if entry.entry_type == 2 and entry.name == name]
        if not matches:
            raise KeyError(name)
        entry = matches[0]
        if entry.size < self.mini_cutoff:
            return self._read_mini_chain(entry.start_sector, entry.size)
        return self._read_chain(entry.start_sector, self.fat, entry.size)


def clean_para_text(payload: bytes) -> str:
    if len(payload) % 2:
        payload = payload[:-1]
    text = payload.decode("utf-16le", errors="replace")
    chars: list[str] = []
    i = 0
    while i < len(text):
        code = ord(text[i])
        if code == 9:
            chars.append("\t")
        elif code in (10, 13):
            chars.append(" ")
        elif code < 32:
            # Most HWP inline controls occupy eight UTF-16 code units.
            if 16 <= code <= 31:
                i += 7
        else:
            chars.append(text[i])
        i += 1
    return "".join(chars).strip()


def extract_paragraphs(section: bytes) -> list[str]:
    paragraphs: list[str] = []
    pos = 0
    while pos + 4 <= len(section):
        header = struct.unpack_from("<I", section, pos)[0]
        pos += 4
        tag_id = header & 0x3FF
        size = (header >> 20) & 0xFFF
        if size == 0xFFF:
            if pos + 4 > len(section):
                break
            size = struct.unpack_from("<I", section, pos)[0]
            pos += 4
        payload = section[pos : pos + size]
        pos += size
        if tag_id == 67:
            value = clean_para_text(payload)
            if value:
                paragraphs.append(value)
    return paragraphs


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("input", type=Path)
    parser.add_argument("--preview-image", type=Path)
    args = parser.parse_args()

    ole = CompoundFile(args.input)
    print("STREAMS")
    for entry in ole.entries:
        if entry.entry_type == 2:
            print(f"{entry.name}\t{entry.size}")

    header = ole.stream("FileHeader")
    flags = struct.unpack_from("<I", header, 36)[0]
    compressed = bool(flags & 1)
    print(f"\nCOMPRESSED\t{compressed}")

    try:
        preview = ole.stream("PrvText").decode("utf-16le", errors="replace").rstrip("\x00")
        print("\nPREVIEW TEXT\n" + preview)
    except KeyError:
        pass

    if args.preview_image:
        preview_image = ole.stream("PrvImage")
        args.preview_image.write_bytes(preview_image)
        print(f"\nPREVIEW IMAGE\t{args.preview_image}\t{len(preview_image)}")

    for entry in ole.entries:
        if entry.entry_type != 2 or not entry.name.startswith("Section"):
            continue
        raw = ole.stream(entry.name)
        if compressed:
            raw = zlib.decompress(raw, -15)
        print(f"\n{entry.name.upper()} PARAGRAPHS")
        for index, paragraph in enumerate(extract_paragraphs(raw), start=1):
            print(f"{index:03d}\t{paragraph}")


if __name__ == "__main__":
    main()
