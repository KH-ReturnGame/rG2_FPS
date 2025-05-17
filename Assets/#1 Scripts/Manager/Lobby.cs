using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class Lobby : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup preparing;

    public void Start()
    {
        preparing.gameObject.SetActive(false);
    }

    public void Tutorial()
    {
        SceneLoader.LoadScene("Main");
    }
    
    public void Preparing()
    {
        StartCoroutine(ShowPreparingText());
    }

    private IEnumerator ShowPreparingText()
    {
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
}