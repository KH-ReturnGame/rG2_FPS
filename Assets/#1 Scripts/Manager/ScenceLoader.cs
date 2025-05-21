using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string nextScene;

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}
