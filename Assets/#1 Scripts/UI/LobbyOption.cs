using UnityEngine;

public class LobbyOption : MonoBehaviour
{
    
    [Header("Game Mode Selector")]
    public GameObject gameModeSelector; 

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
    public void OnClickOption()
    {
        gameModeSelector.SetActive(true);
        optionPanel.SetActive(true);
        WeaponBase.isWeaponInputEnabled = false;
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서는 Play모드 종료
#else
        Application.Quit(); // 빌드 실행 중일 때 종료
#endif
    }
}