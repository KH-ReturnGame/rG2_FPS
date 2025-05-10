using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionPanel;
    
    private bool resumeDelay = false;

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
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        WeaponBase.isWeaponInputEnabled = false;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Input.ResetInputAxes(); // 👈 클릭 입력 상태 완전 초기화
        WeaponBase.isWeaponInputEnabled = true;
    }

    public void OnClickMainMenu() // 시작 화면
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void OnClickRestart() // 재시작
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
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