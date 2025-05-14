using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void GameStart()
    {
        LoadingSceneManager.LoadScene("Lobby");
    }
}
