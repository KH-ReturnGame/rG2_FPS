using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionPanel;
    
    private bool resumeDelay = false;
    private bool isPaused = false;

    void Start()
    {
        if (resumeDelay)
        {
            // 마우스 클릭 방지
            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                resumeDelay = false;
            }
            return;
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // 옵션창 열려 있으면 → 메뉴 
        if (optionPanel.activeSelf)
        {
            optionPanel.SetActive(false);
            pausePanel.SetActive(true);
            return;
        }

        // 일시정지  열려 있으면 → 게임 재개
        if (pausePanel.activeSelf)
        {
            ResumeGame();
            return;
        }

        // 그 외에는 → 일시정지
        PauseGame();
    }


    public void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        optionPanel.SetActive(false); // 옵션 패널 강제 닫기

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AudioListener.pause = true;
        WeaponBase.isWeaponInputEnabled = false;
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        optionPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Input.ResetInputAxes();
        AudioListener.pause = false;
        WeaponBase.isWeaponInputEnabled = true;
    }

    public void OnClickMainMenu() // 시작 화면
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneLoader.LoadScene("Lobby");
    }

    public void OnClickRestart() // 재시작
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneLoader.LoadScene("Main");
    }
    
    public void OnClickOption()
    {
        optionPanel.SetActive(true);
        pausePanel.SetActive(false);
        WeaponBase.isWeaponInputEnabled = false;
    }

    public void OnClickCloseOption()
    {
        optionPanel.SetActive(false);
        pausePanel.SetActive(true);
        WeaponBase.isWeaponInputEnabled = false;
    }
}