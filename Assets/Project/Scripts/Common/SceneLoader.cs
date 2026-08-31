using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private const string LoadingSceneName = "Loading";

    public static string TargetScene { get; private set; }

    public static void LoadScene(string sceneName)
    {
        TargetScene = sceneName;
        SceneManager.LoadScene(LoadingSceneName);
    }
}
