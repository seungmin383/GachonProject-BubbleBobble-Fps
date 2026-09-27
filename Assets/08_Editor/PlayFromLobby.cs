using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayFromLobby
{
    static PlayFromLobby() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        SceneAsset lobby = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/00_Scenes/00_LobbyScene.unity");
        if (lobby != null)
            EditorSceneManager.playModeStartScene = lobby;
    }
}
