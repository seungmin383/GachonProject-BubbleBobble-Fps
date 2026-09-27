/* GPT-시간 만료와 Pop 분리, 반복 포획, 물리 복구, 분노 속도, 점멸을 실제 Unity 프레임에서 확인한다. */
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Asset.Script.Component;
using Asset.Script.Monster;
using Asset.Script.Weapon;
using UnityEditor;
using UnityEngine;

/* GPT-게임 코드에 남아 있는 사용하지 않는 using을 충족하며 게임 동작이나 Unity 물리를 대체하지 않는다. */
namespace Unity.VisualScripting { internal static class UnusedImport { } }

public class EscapeValidationDriver : MonoBehaviour
{
    /* GPT-실패 시 예외를 보고서에 기록해 배치 실행이 조용히 멈추지 않게 한다. */
    private IEnumerator Start()
    {
        IEnumerator checks = Check();
        while (true)
        {
            bool next;
            try { next = checks.MoveNext(); }
            catch (Exception error)
            {
                File.WriteAllText("result.txt", "FAIL\n" + error);
                Debug.LogException(error);
                EditorApplication.Exit(1);
                yield break;
            }
            if (!next) break;
            yield return checks.Current;
        }
        File.WriteAllText("result.txt", "PASS: 10s expiry, 5s angry recapture, repeated 2x speed, attack return, physics restore, empty expiry, Pop only kill, pause, accelerating per-bubble blink with alpha preserved.");
        EditorApplication.Exit(0);
    }

    /* GPT-비공개 Unity 콜백에 동일한 입력을 전달하되 실제 Update와 Rigidbody를 사용해 상태 전이를 검증한다. */
    private IEnumerator Check()
    {
        Time.timeScale = 5.0f;
        GameObject target = new GameObject("target");
        target.transform.position = Vector3.forward * 1000;
        GameObject enemy = new GameObject("enemy");
        enemy.SetActive(false);
        Rigidbody body = enemy.AddComponent<Rigidbody>();
        body.useGravity = false;
        Collider collider = enemy.AddComponent<BoxCollider>();
        Capturable capture = enemy.AddComponent<Capturable>();
        BasicMonster monster = enemy.AddComponent<BasicMonster>();
        Set(monster, "_capturable", capture);
        Set(monster, "_target", target.transform);
        enemy.SetActive(true);
        yield return null;
        AssertSpeed(monster, 3);

        int escaped = 0, killed = 0;
        capture.Escaped += () => escaped++;
        capture.BubbleBurst += () => killed++;
        Bubble empty = NewBubble();
        Bubble bubble = NewBubble();
        Set(bubble, "_remainingTime", 1.0f);
        Invoke(bubble, "OnTriggerEnter", collider);
        Assert(Mathf.Approximately(bubble.RemainingTime, 10), "capture must reset to 10 seconds");
        Assert(body.isKinematic && State(monster) == "Captured", "capture must suspend movement/physics");
        float started = Time.time;
        while (bubble != null && Time.time - started < 11) yield return null;
        Assert(bubble == null && Time.time - started >= 9.9f, "normal expiry must take 10 seconds");
        Assert(empty == null, "empty bubble must expire");
        Assert(escaped == 1 && killed == 0 && enemy != null, "expiry must release, never kill");
        Assert(!body.isKinematic && capture.IsAngry && State(monster) == "Angry", "escape must restore physics and enter Angry");
        AssertSpeed(monster, 6);
        Vector3 releasedPosition = enemy.transform.position;
        Invoke(capture, "LateUpdate");
        Assert(enemy.transform.position == releasedPosition, "released target must stop following bubble");

        target.transform.position = enemy.transform.position;
        Invoke(monster, "UpdateState");
        Assert(State(monster) == "Attack", "angry target must still attack");
        target.transform.position = Vector3.forward * 1000;
        Invoke(monster, "UpdateState");
        Assert(State(monster) == "Angry", "attack must return to Angry");

        for (int attempt = 0; attempt < 2; attempt++)
        {
            bubble = NewBubble();
            Invoke(bubble, "OnTriggerEnter", collider);
            Assert(Mathf.Approximately(bubble.RemainingTime, 5), "angry recapture must use 5 seconds");
            started = Time.time;
            while (bubble != null && Time.time - started < 6) yield return null;
            Assert(bubble == null && Time.time - started >= 4.9f, "angry expiry must take 5 seconds");
            Assert(escaped == attempt + 2 && killed == 0, "repeated expiry must only escape");
            AssertSpeed(monster, 6);
        }

        bubble = NewBubble();
        Invoke(bubble, "OnTriggerEnter", collider);
        Time.timeScale = 0;
        yield return null;
        float pausedTime = bubble.RemainingTime;
        yield return new WaitForSecondsRealtime(0.15f);
        Assert(Mathf.Approximately(bubble.RemainingTime, pausedTime), "pause must freeze expiry");
        bubble.Burst();
        bubble.Burst();
        Invoke(bubble, "Finish", true);
        Assert(killed == 1 && escaped == 3, "Pop must kill exactly once without escape");
        yield return null;
        Assert(enemy == null, "Pop must remove monster");
        Time.timeScale = 1;

        GameObject fixedTarget = new GameObject("kinematic target");
        Rigidbody fixedBody = fixedTarget.AddComponent<Rigidbody>();
        fixedBody.isKinematic = true;
        Capturable fixedCapture = fixedTarget.AddComponent<Capturable>();
        Bubble fixedBubble = NewBubble();
        Assert(fixedCapture.TryCapture(fixedBubble), "kinematic target must capture");
        fixedCapture.Escape();
        Assert(fixedBody.isKinematic, "original kinematic flag must be restored");

        GameObject visual = new GameObject("visual");
        visual.SetActive(false);
        Renderer renderer = visual.AddComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Validation/Color"));
        Color original = new Color(0.3f, 1, 0.9f, 0.2f);
        material.SetColor("_BaseColor", original);
        renderer.sharedMaterial = material;
        Bubble visualBubble = visual.AddComponent<Bubble>();
        BubbleWarning warning = visual.AddComponent<BubbleWarning>();
        visual.SetActive(true);
        visualBubble.enabled = false;
        warning.enabled = false;
        int slow = 0, fast = 0;
        for (int phase = 0; phase < 2; phase++)
        {
            Invoke(warning, "OnDisable");
            Set(visualBubble, "_remainingTime", phase == 0 ? 5.0f : 1.0f);
            bool lastRed = false;
            int toggles = 0;
            float end = Time.time + 1.1f;
            while (Time.time < end)
            {
                Invoke(warning, "Update");
                bool red = (bool)Get(warning, "_isRed");
                if (red != lastRed) toggles++;
                lastRed = red;
                yield return null;
            }
            if (phase == 0) slow = toggles; else fast = toggles;
        }
        Assert(fast > slow * 2, "1-second warning must blink faster than 5-second warning");
        Invoke(warning, "OnDisable");
        Set(visualBubble, "_remainingTime", 5.0f);
        Invoke(warning, "Update");
        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(properties);
        Color redColor = properties.GetColor("_BaseColor");
        Assert(redColor.r == 1 && redColor.g == 0 && Mathf.Approximately(redColor.a, 0.2f), "warning must preserve transparency");
        Assert(material.GetColor("_BaseColor") == original, "shared material must not change");
        Set(visualBubble, "_remainingTime", 10.0f);
        Invoke(warning, "Update");
        renderer.GetPropertyBlock(properties);
        Assert(properties.GetColor("_BaseColor") == original, "capture timer reset must restore original color");
    }

    /* GPT-테스트 위치가 바뀌지 않도록 이동만 정지한 실제 Bubble 컴포넌트를 만든다. */
    private static Bubble NewBubble()
    {
        Bubble bubble = new GameObject("bubble").AddComponent<Bubble>();
        Set(bubble, "_speed", 0.0f);
        Set(bubble, "_floatingSpeed", 0.0f);
        return bubble;
    }

    /* GPT-한 번의 실제 추적 호출에서 이동 거리를 측정해 기본 속도와 비중첩 2배 속도를 확인한다. */
    private static void AssertSpeed(BasicMonster monster, float expected)
    {
        Vector3 position = monster.transform.position;
        Invoke(monster, "Chase");
        float speed = Vector3.Distance(position, monster.transform.position) / Time.deltaTime;
        Assert(Mathf.Abs(speed - expected) < 0.02f, "unexpected speed: " + speed);
    }

    /* GPT-테스트 전용 리플렉션으로 게임 코드에 공개 테스트 API를 추가하지 않고 상태를 관찰한다. */
    private static void Set(object item, string field, object value) => item.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(item, value);
    private static object Get(object item, string field) => item.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(item);
    private static void Invoke(object item, string method, params object[] args) => item.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(item, args);
    private static string State(BasicMonster monster) => typeof(MonsterBase).GetField("_currentState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(monster).ToString();
    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
}
