using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Lobby : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup preparing;

    [Header("Game Mode Selector")]
    public GameObject gameModeSelector; 

    public void Start()
    {
        preparing.gameObject.SetActive(false);
        gameModeSelector.SetActive(false); // 시작 시 비활성화
    }

    public void Tutorial()
    {
        gameModeSelector.SetActive(false);
        SceneLoader.LoadScene("Main");
    }

    public void Preparing()
    {
        gameModeSelector.SetActive(false);
        StartCoroutine(ShowPreparingText());
    }

    private IEnumerator ShowPreparingText()
    {
        gameModeSelector.SetActive(false);
        preparing.gameObject.SetActive(true);

        float speed = 8f;
        for (float a = 0; a < 1f; a += Time.deltaTime * speed)
        {
            preparing.alpha = a;
            yield return null;
        }
        preparing.alpha = 1f;

        yield return new WaitForSeconds(0.5f);

        for (float a = 1f; a > 0f; a -= Time.deltaTime * speed)
        {
            preparing.alpha = a;
            yield return null;
        }
        preparing.alpha = 0f;

        preparing.gameObject.SetActive(false);
    }

    public void OpenGameModeSelector()
    {
        gameModeSelector.SetActive(true);
    }
}