/* GPT-벽과 바닥의 즉시 폭발, 천장 정지와 밀려난 뒤 상승, 비행 버블 간 통과를 실제 Unity 물리로 검증한다. */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Asset.Script.Weapon;
using UnityEditor;
using UnityEngine;

public class ACollisionValidationDriver : MonoBehaviour
{
    /* GPT-각 물리 검증 사례가 끝날 때 생성한 오브젝트를 정리해 다음 사례에 영향을 주지 않게 한다. */
    private readonly List<GameObject> _objects = new List<GameObject>();
    private int _passed;

    /* GPT-검증 실패의 조건과 예외를 파일에 남겨 배치 실행의 성공 여부를 명확하게 판정한다. */
    private IEnumerator Start()
    {
        IEnumerator checks = Check();
        while (true)
        {
            bool next;
            try { next = checks.MoveNext(); }
            catch (Exception error)
            {
                File.WriteAllText("result.txt", "FAIL after " + _passed + " checks\n" + error);
                Debug.LogException(error);
                EditorApplication.Exit(1);
                yield break;
            }
            if (!next) break;
            yield return checks.Current;
        }
        File.WriteAllText("result.txt", "PASS " + _passed + " checks: high-speed scaled wall hit, floor hit, ceiling stop, physical push and resume, wall spawn overlap, ceiling spawn overlap, flying pass-through, floating push.");
        EditorApplication.Exit(0);
    }

    /* GPT-이동과 충돌 콜백을 수동으로 대체하지 않고 생성한 실제 콜라이더에서 요구 동작을 관찰한다. */
    private IEnumerator Check()
    {
        /* GPT-프리팹과 같은 3배 크기의 빠른 버블이 한 프레임 안에 얇은 벽을 넘더라도 터지는지 확인한다. */
        Obstacle(new Vector3(0, 0, 4), new Vector3(10, 10, 0.1f), "Wall");
        Bubble bubble = NewBubble(Vector3.zero, Vector3.forward, 300, 3);
        yield return new WaitForSeconds(0.08f);
        Assert(bubble == null, "high-speed scaled bubble must burst at thin Wall");
        Pass(); Clear(); yield return null;

        /* GPT-Default 레이어의 바닥을 향해 발사한 버블이 접촉 즉시 제거되는지 확인한다. */
        Obstacle(Vector3.zero, new Vector3(10, 0.1f, 10), "Default");
        bubble = NewBubble(Vector3.up * 2, Vector3.down, 40);
        yield return new WaitForSeconds(0.1f);
        Assert(bubble == null, "downward bubble must burst at floor");
        Pass(); Clear(); yield return null;

        /* GPT-천장 충돌 직후 높이를 고정하고 비행 속도가 남지 않는지 확인한다. */
        Obstacle(Vector3.up * 4, new Vector3(2, 0.1f, 4), "Default");
        bubble = NewBubble(Vector3.zero, Vector3.up);
        yield return new WaitForSeconds(0.3f);
        Assert(bubble != null && State(bubble) == "CeilingBlocked", "ceiling hit must enter CeilingBlocked");
        float ceilingHeight = bubble.GetComponent<Rigidbody>().position.y;
        Assert(Mathf.Abs(ceilingHeight - 3.45f) < 0.04f, "bubble must stop below ceiling at its radius: " + ceilingHeight);
        yield return new WaitForSeconds(0.2f);
        Assert(State(bubble) == "CeilingBlocked" && Mathf.Abs(bubble.GetComponent<Rigidbody>().position.y - ceilingHeight) < 0.02f,
            "ceiling body must keep its height");
        Pass();

        /* GPT-다른 비행 버블의 실제 충돌로 천장 버블이 옆으로 밀리고 천장 밖에서 다시 상승하는지 확인한다. */
        NewBubble(new Vector3(-2, ceilingHeight - 0.25f, 0), Vector3.right, 30);
        yield return new WaitForSeconds(0.7f);
        Assert(bubble != null && State(bubble) == "Floating" && bubble.GetComponent<Rigidbody>().position.y > ceilingHeight + 0.05f,
            "pushed ceiling body must leave and resume: " + (bubble == null ? "destroyed" : State(bubble) + " " + bubble.GetComponent<Rigidbody>().position));
        Pass(); Clear(); yield return null;

        /* GPT-SphereCast 시작점이 벽 안인 발사도 누락 없이 즉시 폭발하는지 확인한다. */
        Obstacle(Vector3.right * 0.25f, new Vector3(0.1f, 6, 6), "Wall");
        bubble = NewBubble(Vector3.zero, Vector3.right);
        yield return new WaitForSeconds(0.04f);
        Assert(bubble == null, "spawn overlapping wall must burst");
        Pass(); Clear(); yield return null;

        /* GPT-천장 내부에서 생성된 버블은 겹침을 해소하고 천장 아래에 머무는지 확인한다. */
        Obstacle(Vector3.up * 4, new Vector3(4, 0.1f, 4), "Default");
        bubble = NewBubble(Vector3.up * 3.6f, Vector3.up);
        yield return new WaitForSeconds(0.1f);
        Assert(bubble != null && State(bubble) == "CeilingBlocked" && Mathf.Abs(bubble.GetComponent<Rigidbody>().position.y - 3.45f) < 0.04f,
            "ceiling spawn overlap must be corrected and held");
        Pass(); Clear(); yield return null;

        /* GPT-서로 마주 오는 비행 버블이 통과하면서 Flying 상태를 유지하는지 확인한다. */
        Bubble first = NewBubble(Vector3.left * 2, Vector3.right);
        Bubble second = NewBubble(Vector3.right * 2, Vector3.left);
        yield return new WaitForSeconds(0.25f);
        Assert(first != null && second != null && State(first) == "Flying" && State(second) == "Flying"
            && first.GetComponent<Rigidbody>().position.x > 0 && second.GetComponent<Rigidbody>().position.x < 0,
            "flying bubbles must pass each other");
        Pass(); Clear(); yield return null;

        /* GPT-비행 버블이 상승 버블을 밀어도 비행을 계속하는 기존 상호작용을 확인한다. */
        Bubble floating = NewBubble(Vector3.zero, Vector3.up);
        Set(floating, "_floatingSpeed", 0.1f);
        typeof(Bubble).GetMethod("BeginFloating", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(floating, null);
        first = NewBubble(Vector3.left * 2, Vector3.right);
        yield return new WaitForSeconds(0.2f);
        Assert(first != null && floating != null && State(first) == "Flying" && State(floating) == "Floating"
            && first.GetComponent<Rigidbody>().position.x > 0 && floating.GetComponent<Rigidbody>().position.x > 0.1f,
            "flying body must continue and push floating body");
        Pass();
    }

    /* GPT-실제 프리팹과 같은 포획 Trigger, 자식 물리 SphereCollider, Rigidbody 구조를 만든 뒤 Awake를 실행한다. */
    private Bubble NewBubble(Vector3 position, Vector3 direction, float speed = 20, float scale = 1)
    {
        GameObject root = new GameObject("bubble");
        _objects.Add(root);
        root.SetActive(false);
        root.transform.position = position;
        root.transform.rotation = Quaternion.LookRotation(direction, Mathf.Abs(direction.y) > 0.9f ? Vector3.forward : Vector3.up);
        root.transform.localScale = Vector3.one * scale;
        Rigidbody rigidBody = root.AddComponent<Rigidbody>();
        rigidBody.linearDamping = 2;
        root.AddComponent<SphereCollider>().isTrigger = true;
        GameObject child = new GameObject("BubbleBody");
        child.transform.SetParent(root.transform, false);
        SphereCollider collider = child.AddComponent<SphereCollider>();
        Bubble bubble = root.AddComponent<Bubble>();
        Set(bubble, "_bodyCollider", collider);
        Set(bubble, "_environmentLayers", (LayerMask)LayerMask.GetMask("Default", "Wall"));
        Set(bubble, "_lifeTime", 60f);
        Set(bubble, "_timeToFloating", 30f);
        Set(bubble, "_speed", speed);
        Set(bubble, "_chainRadius", 0.05f);
        root.SetActive(true);
        Physics.SyncTransforms();
        Assert(bubble.enabled, "test body must initialize");
        return bubble;
    }

    /* GPT-정적인 환경 BoxCollider를 실제 프로젝트와 같은 레이어에 배치해 환경 검사를 검증한다. */
    private void Obstacle(Vector3 position, Vector3 size, string layer)
    {
        GameObject obstacle = new GameObject("environment");
        _objects.Add(obstacle);
        obstacle.transform.position = position;
        obstacle.layer = LayerMask.NameToLayer(layer);
        obstacle.AddComponent<BoxCollider>().size = size;
        Physics.SyncTransforms();
    }

    /* GPT-각 성공 조건의 개수를 기록하고 검증 전용 오브젝트만 정리한다. */
    private void Pass() { _passed++; Debug.Log("A validation passed " + _passed); }
    private void Clear() { foreach (GameObject item in _objects) if (item != null) Destroy(item); _objects.Clear(); }

    /* GPT-비공개 상태는 관찰용 리플렉션으로 확인해 게임 코드에 테스트 API를 추가하지 않는다. */
    private static void Set(Bubble bubble, string name, object value) => typeof(Bubble).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(bubble, value);
    private static string State(Bubble bubble) => typeof(Bubble).GetField("_motionState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bubble).ToString();
    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
}
