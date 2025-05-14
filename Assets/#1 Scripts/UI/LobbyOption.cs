using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyOption : MonoBehaviour
{
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
            return;
        }
    }


    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Input.ResetInputAxes(); // 👈 클릭 입력 상태 완전 초기화
        WeaponBase.isWeaponInputEnabled = true;
    }

    public void OnClickMainMenu() // 시작 화면
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Lobby");
    }

    public void OnClickRestart() // 재시작
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }
    
    public void OnClickOption()
    {
        optionPanel.SetActive(true);
        WeaponBase.isWeaponInputEnabled = false;
    }

    public void OnClickCloseOption()
    {
        optionPanel.SetActive(false);
        WeaponBase.isWeaponInputEnabled = false;
    }
}