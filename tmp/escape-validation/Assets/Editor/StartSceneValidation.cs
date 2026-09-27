/* GPT-전투 씬이 열려 있어도 에디터 Play가 로비 씬에서 시작되는지 독립 Unity 프로젝트에서 확인한다. */
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class StartSceneValidation
{
    /* GPT-기본 도메인 재로딩 후에도 Play 진입 결과를 수집해 실제 에디터 설정과 동일하게 검증한다. */
    static StartSceneValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("StartSceneValidation.Pending", false))
                return;
            SessionState.EraseBool("StartSceneValidation.Pending");
            string actual = SceneManager.GetActiveScene().name;
            bool success = actual == "00_LobbyScene";
            File.WriteAllText("start-result.txt", (success ? "PASS: " : "FAIL: ") + actual);
            EditorApplication.Exit(success ? 0 : 1);
        };
    }

    /* GPT-테스트용 로비와 전투 씬을 만든 뒤 전투 씬에서 Play 전환을 재현한다. */
    public static void Run()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single), "Assets/00_Scenes/00_LobbyScene.unity");
        EditorSceneManager.SaveScene(EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single), "Assets/00_Scenes/02_BattleScene.unity");
        EditorSceneManager.playModeStartScene = null;
        /* GPT-실제 프로젝트의 기본 Play 설정과 같이 도메인을 다시 로딩해 시작 씬 설정이 유지되는지 확인한다. */
        EditorSettings.enterPlayModeOptionsEnabled = false;
        SessionState.SetBool("StartSceneValidation.Pending", true);
        EditorApplication.EnterPlaymode();
    }
}
