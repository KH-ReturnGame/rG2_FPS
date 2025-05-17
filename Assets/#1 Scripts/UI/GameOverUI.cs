using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    
    [Header("플레이어 연결")]
    public PlayerStatus playerStatus;
    
    [Header("UI")]
    public GameObject gameOverPanel;
    public CanvasGroup canvasGroup; // 꼭 연결해야 함

    [Header("버튼")]
    public Button retryButton;
    public Button menuButton;
    public Button quitButton;
    
    private bool isGameOverShown = false;
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
    private void Awake()
    {
        retryButton.onClick.AddListener(OnRetryClicked);
        menuButton.onClick.AddListener(OnMenuClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        gameOverPanel.SetActive(false);

        // HP 이벤트 구독
        playerStatus.onHPEvent.AddListener(OnHpChanged);
    }

    private void OnHpChanged(float previous, float current)
    {
        if (!isGameOverShown && current <= 0)
        {
            isGameOverShown = true;
            ShowGameOverUI();
        }
    }

    public void ShowGameOverUI()
    {
        StartCoroutine(FadeInGameOverUI());
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private IEnumerator FadeInGameOverUI()
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        yield return new WaitForSecondsRealtime(0.5f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 1f);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnRetryClicked()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadScene("Main");
    }

    public void OnMenuClicked()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadScene("Main Menu");
    }
    
    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

