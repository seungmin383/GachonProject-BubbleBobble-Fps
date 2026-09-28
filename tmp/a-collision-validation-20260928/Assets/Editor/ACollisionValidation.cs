/* GPT-별도 Unity 프로젝트에서 실제 물리 프레임을 실행해 원본 에디터를 건드리지 않고 A 작업을 검증한다. */
using UnityEditor;
using UnityEngine;

public static class ACollisionValidation
{
    /* GPT-검증 중 도메인 재로드를 생략하고 플레이 모드 진입 후 실제 Bubble을 사용하는 검증기를 생성한다. */
    public static void Run()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                new GameObject("A collision validation").AddComponent<ACollisionValidationDriver>();
        };
        EditorApplication.EnterPlaymode();
    }
}
