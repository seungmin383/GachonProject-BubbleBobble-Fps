/* GPT-어느 씬을 편집 중이어도 Play 버튼은 GameManager가 있는 로비에서 시작하도록 설정한다. */
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayFromLobby
{
    /* GPT-에디터가 Play 모드로 전환될 때 시작 씬을 지정해 Shift와 Enter 입력 흐름을 보장한다. */
    static PlayFromLobby() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

    /* GPT-기존에 열어 둔 전투 씬을 변경하지 않고 Play 세션에서만 로비 씬을 먼저 실행한다. */
    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        SceneAsset lobby = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/00_Scenes/00_LobbyScene.unity");
        if (lobby != null)
            EditorSceneManager.playModeStartScene = lobby;
    }
}
