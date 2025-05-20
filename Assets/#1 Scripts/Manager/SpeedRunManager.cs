using System;
using UnityEngine;

public class SpeedRunManager : MonoBehaviour
{
    public static SpeedRunManager Instance;
    public float totalTime = 0f;
    private bool isRecording = false;
    public bool isSpeedRun = false;
    
    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void SpeedRunBtn()
    {
        isSpeedRun = true;
        GameModeSelector.Instance.StartScene();
    }

    private void Update()
    {
        if (isRecording && isSpeedRun)
        {
            totalTime += Time.deltaTime;
        }
    }

    public void StartSpeedRun()
    {
        isRecording = true;
    }

    public float EndSpeedRun()
    {
        isRecording = false;
        float mySpeedRun = totalTime;
        totalTime = 0;
        return mySpeedRun;
    }
}
