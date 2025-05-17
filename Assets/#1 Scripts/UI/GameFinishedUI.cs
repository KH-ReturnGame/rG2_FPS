using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameFinishedUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameFinishedPanel;
    public CanvasGroup canvasGroup; // 꼭 연결해야 함

    [Header("버튼")]
    public Button retryButton;
    public Button menuButton;
    public Button quitButton;
    
    private bool isGameFinishedShown = false;
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
        retryButton.onClick.AddListener(OnContinueClicked);
        menuButton.onClick.AddListener(OnMenuClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        gameFinishedPanel.SetActive(false);
    }

    // 이 함수는 플레이어가 완료 지점에 도달했을 때 호출됨
    public void ShowGameFinishedUI()
    {
        if (!isGameFinishedShown)
        {
            isGameFinishedShown = true;
            StartCoroutine(FadeInGameFinishedUI());
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private IEnumerator FadeInGameFinishedUI()
    {
        Time.timeScale = 0f;
        gameFinishedPanel.SetActive(true);
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

    public void OnContinueClicked()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadScene("Main");
    }

    public void OnMenuClicked()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadScene("MenuScene");
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