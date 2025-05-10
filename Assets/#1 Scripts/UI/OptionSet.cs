using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OptionSet : MonoBehaviour
{
    [Header("감도")]
    public Slider sensitivitySlider;
    public float currentSensitivity = 1.0f;
    public TMP_Text sensitivityValueText;

    [Header("볼륨")]
    public Slider volumeSlider;
    public TMP_Text volumeValueText;
    public AudioSource[] allAudioSources; // 전체 대상 소리

    [Header("해상도")]
    public TMP_Dropdown resolutionDropdown;

    private readonly Vector2Int[] resolutionOptions = new Vector2Int[] // 해상도 초기화
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1280, 720),
        new Vector2Int(640, 480)
    };

    private void Start()
    {
        // 초기값 로드
        sensitivitySlider.minValue = 0.01f;
        sensitivitySlider.maxValue = 2.0f;

        currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        sensitivitySlider.value = currentSensitivity;
        sensitivityValueText.text = currentSensitivity.ToString("F2"); // 🔹 소수점 2자리
        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);

        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        volumeSlider.value = savedVolume;
        volumeValueText.text = Mathf.RoundToInt(savedVolume * 100) + "%";
        volumeSlider.onValueChanged.AddListener(UpdateVolume);

        // 해상도 옵션 채우기
        resolutionDropdown.ClearOptions();
        var labels = new List<string>();
        foreach (var res in resolutionOptions)
            labels.Add($"{res.x} × {res.y}");
        resolutionDropdown.AddOptions(labels);

        // 기본값 설정 (0번째가 1080p라 가정)
        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue(); // 🔹 UI 갱신
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);

        // 초기 해상도 적용
        ChangeResolution(savedIndex);
    }

    private void UpdateSensitivity(float value)
    {
        currentSensitivity = value;
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        sensitivityValueText.text = value.ToString("F2");
    }

    public void UpdateVolume(float value)
    {
        foreach (var src in allAudioSources)
        {
            if (src != null) src.volume = value;
        }

        volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void ChangeResolution(int index)
    {
        Vector2Int res = resolutionOptions[index];
        Screen.SetResolution(res.x, res.y, FullScreenMode.Windowed);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }
}
