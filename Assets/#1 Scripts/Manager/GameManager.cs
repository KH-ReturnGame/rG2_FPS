using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject SpeedRunText;
    private void Start()
    {
        if (SpeedRunManager.Instance.isSpeedRun)
        {
            SpeedRunManager.Instance.StartSpeedRun();
            SpeedRunText.SetActive(true);
            SpeedRunManager.Instance.totalTime = 0f;
        }
    }

    public void Update()
    {
        if (SpeedRunManager.Instance.isSpeedRun)
        {
            SpeedRunText.GetComponent<TextMeshProUGUI>().text = SpeedRunManager.Instance.totalTime.ToString("F") + "s";
        }
    }
}
