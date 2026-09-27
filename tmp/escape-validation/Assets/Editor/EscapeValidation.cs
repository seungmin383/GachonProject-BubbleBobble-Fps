/* GPT-원본 프로젝트를 열어 둔 상태에서 복사한 실제 게임 코드를 별도 Unity 프로젝트의 플레이 모드로 검증한다. */
using UnityEditor;
using UnityEngine;

public static class EscapeValidation
{
    /* GPT-도메인 재로딩 없이 검증 컴포넌트를 시작해 완료 결과와 종료 코드를 수집한다. */
    public static void Run()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                new GameObject("Escape validation").AddComponent<EscapeValidationDriver>();
        };
        EditorApplication.EnterPlaymode();
    }
}
