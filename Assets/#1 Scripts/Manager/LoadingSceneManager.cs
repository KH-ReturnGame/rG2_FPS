using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;

    [Header("Loading Scene")]
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
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneLoader.nextScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            // 실제 진행률
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);

            // 부드럽게 설정
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
                yield return new WaitForSeconds(0.3f);
                progressText.text = "100%";
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}