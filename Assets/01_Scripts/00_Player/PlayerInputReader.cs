using UnityEngine;
using Asset.Script.Player;

namespace Asset.Script.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        private PlayerInputAction _inputAction;

        /* 입력 없음 → ( 0, 0)
            W        → ( 0, 1)
            S        → ( 0,-1)
            A        → (-1, 0)
            D        → ( 1, 0) */
        public Vector2 CurrentInput => _inputAction.Player.Move.ReadValue<Vector2>();
        public bool PressedJump => _inputAction.Player.Jump.WasPressedThisFrame();

        /* 마우스 오른쪽 이동 → (양수, 0)
           마우스 왼쪽 이동   → (음수, 0)
           마우스 위 이동     → (0, 양수)
           마우스 아래 이동   → (0, 음수)*/
        public Vector2 CurrentLook => _inputAction.Player.Look.ReadValue<Vector2>();

        public bool LeftMouseInput => _inputAction.Player.LeftMouseInput.WasPressedThisFrame();
        //public bool RightMouseInput => _inputAction.Player.RightMouseInput.WasPressedThisFrame();

        private void Awake()
        {
            _inputAction = new PlayerInputAction();
        }

        private void OnEnable()
        {
            _inputAction.Player.Enable();
        }

        private void OnDisable()
        {
            _inputAction.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputAction?.Dispose();
        }
    }

}