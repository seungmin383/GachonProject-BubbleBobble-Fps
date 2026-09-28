using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromLobby
{
    private const string MenuPath = "Tools/Play From Lobby";
    private static readonly string PreferenceKey = Application.dataPath + ".PlayFromLobby";
    private const string ScenePath = "Assets/00_Scenes/00_LobbyScene.unity";

    static PlayFromLobby()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        ApplyStartScene();
    }

    [MenuItem(MenuPath)]
    private static void TogglePlayFromLobby()
    {
        /* 토글 */
        EditorPrefs.SetBool(PreferenceKey, !EditorPrefs.GetBool(PreferenceKey, false));
        ApplyStartScene();
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidatePlayFromLobby()
    {
        /* 현재 설정값에 따라 v 표시를 붙이거나 끔 */
        Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PreferenceKey, false));

        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        /* Play 들어가기 직전 */
        if (state != PlayModeStateChange.ExitingEditMode)
        {
            return;
        }

        ApplyStartScene();
    }

    private static void ApplyStartScene()
    {
        EditorSceneManager.playModeStartScene = EditorPrefs.GetBool(PreferenceKey, false)
            ? AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)
            : null;
    }
}
