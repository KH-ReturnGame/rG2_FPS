using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;

    [Header("🔄 로딩 UI")]
    public Slider progressBar;
    public TMP_Text progressText;

    private float currentProgress = 0f;

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    private void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            // 실제 진행률
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);

            // Lerp로 부드럽게 보간
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 5f);

            // UI 반영
            if (progressBar != null)
                progressBar.value = currentProgress;

            if (progressText != null)
                progressText.text = $"{Mathf.RoundToInt(currentProgress * 100f)}%";

            // 거의 다 로드되면 씬 전환
            if (currentProgress >= 0.995f)
            {
                progressBar.value = 1f;
                progressText.text = "100%";
                yield return new WaitForSeconds(0.3f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}