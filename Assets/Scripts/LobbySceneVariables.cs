using UnityEngine.SceneManagement;

public static class LobbySceneVariables
{
    public static string NextScene { get; private set; } = "MainScene";

    public static void SetNextScene(string sceneName)
    {
        NextScene = sceneName;
    }
}
