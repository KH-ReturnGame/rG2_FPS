using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void GameStart()
    {
        SceneManager.LoadScene("Main");
    }
}
