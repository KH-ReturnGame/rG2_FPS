using UnityEngine;
public class MainMenu : MonoBehaviour
{
    public void GameStart()
    {
        SceneLoader.LoadScene("Lobby");
    }
}
