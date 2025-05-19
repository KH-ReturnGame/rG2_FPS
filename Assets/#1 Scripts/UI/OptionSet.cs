using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class OptionManager : MonoBehaviour
{
    [Header("🎮 감도")]
    public Slider sensitivitySlider;
    public TMP_Text sensitivityValueText;
    private float currentSensitivity;

    [Header("🔊 볼륨")]
    public Slider volumeSlider;
    public TMP_Text volumeValueText;
    public AudioSource[] allAudioSources;

    [Header("🖥️ 해상도")]
    public TMP_Dropdown resolutionDropdown;
    private readonly Vector2Int[] resolutionOptions = new Vector2Int[]
    {
        new Vector2Int(3840, 2160),
        new Vector2Int(1920, 1080),
        new Vector2Int(1280, 720),
        new Vector2Int(640, 480)
    };

    [Header("🪟 화면 모드")]
    public TMP_Dropdown screenModeDropdown;

    [Header("⏱️ 프레임 설정")]
    public TMP_Dropdown frameRateDropdown;
    public Toggle vSyncToggle;
    public CanvasGroup frameDropdownCanvasGroup;
    private readonly int[] frameRates = { 30, 60, 120, -1 };

    [Header("📂 옵션 패널")]
    public GameObject optionsPanel;

    private void Start()
    {
        // 감도 설정
        currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        sensitivitySlider.minValue = 0.01f;
        sensitivitySlider.maxValue = 2.0f;
        sensitivitySlider.value = currentSensitivity;
        sensitivityValueText.text = currentSensitivity.ToString("F2");
        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);

        // 볼륨 설정
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        volumeSlider.value = savedVolume;
        volumeValueText.text = Mathf.RoundToInt(savedVolume * 100) + "%";
        volumeSlider.onValueChanged.AddListener(UpdateVolume);

        // 해상도 드롭다운
        resolutionDropdown.ClearOptions();
        List<string> labels = new();
        foreach (var res in resolutionOptions)
            labels.Add($"{res.x} × {res.y}");
        resolutionDropdown.AddOptions(labels);
        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        // 화면 모드 드롭다운
        int savedModeIndex = PlayerPrefs.GetInt("ScreenMode", 0);
        screenModeDropdown.value = savedModeIndex;
        screenModeDropdown.RefreshShownValue();
        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);

        // 프레임 설정
        int savedFrameIndex = PlayerPrefs.GetInt("FrameRate", 2); // 기본: 120
        bool savedVSync = PlayerPrefs.GetInt("VSync", 1) == 1;
        frameRateDropdown.value = savedFrameIndex;
        frameRateDropdown.RefreshShownValue();
        vSyncToggle.isOn = savedVSync;
        frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
        vSyncToggle.onValueChanged.AddListener(OnVSyncToggleChanged);

        // 모든 설정 적용
        ApplyResolution(savedResIndex, savedModeIndex);
        ApplyFrameRate(savedFrameIndex, savedVSync);
    }

    // ---------------------- 감도 & 볼륨 ----------------------
    private void UpdateSensitivity(float value)
    {
        currentSensitivity = value;
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        sensitivityValueText.text = value.ToString("F2");
    }

    private void UpdateVolume(float value)
    {
        foreach (var src in allAudioSources)
            if (src != null) src.volume = value;

        volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    // ---------------------- 해상도 & 화면 모드 ----------------------
    private void OnResolutionChanged(int index)
    {
        PlayerPrefs.SetInt("ResolutionIndex", index);
        ApplyResolution(index, screenModeDropdown.value);
    }

    private void OnScreenModeChanged(int index)
    {
        PlayerPrefs.SetInt("ScreenMode", index);
        ApplyResolution(resolutionDropdown.value, index);
    }

    private void ApplyResolution(int resIndex, int modeIndex)
    {
        Vector2Int res = resolutionOptions[resIndex];
        FullScreenMode mode;

        // 이 조건은 사실상 필요 없음. 모든 해상도에서 모드 변경 허용해도 무방
        if (res.x <= 3840 && res.y <= 2160)
        {
            mode = (modeIndex == 1) ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
        }
        else
        {
            mode = modeIndex switch
            {
                0 => FullScreenMode.ExclusiveFullScreen,
                1 => FullScreenMode.Windowed,
                2 => FullScreenMode.FullScreenWindow,
                _ => FullScreenMode.ExclusiveFullScreen
            };
        }

        // 필요 시 보정값 추가 가능 (Windowed에서 확대 방지)
        Screen.SetResolution(res.x, res.y, mode);
        Screen.fullScreenMode = mode;

        Debug.Log($"[ResolutionSet] {res.x}x{res.y}, {mode}");
    }



    // ---------------------- 프레임레이트 & VSync ----------------------
    private void OnFrameRateChanged(int index)
    {
        PlayerPrefs.SetInt("FrameRate", index);
        ApplyFrameRate(index, vSyncToggle.isOn);
    }

    private void OnVSyncToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("VSync", isOn ? 1 : 0);
        ApplyFrameRate(frameRateDropdown.value, isOn);
    }

    private void ApplyFrameRate(int frameIndex, bool vSyncOn)
    {
        if (vSyncOn)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = frameRates[frameIndex];
        }

        bool interactable = !vSyncOn;
        frameRateDropdown.interactable = interactable;
        if (frameDropdownCanvasGroup != null)
            frameDropdownCanvasGroup.alpha = interactable ? 1f : 0.5f;

        Debug.Log($"[FrameSet] FrameRate: {Application.targetFrameRate}, VSync: {vSyncOn}");
    }

    // ---------------------- 옵션 패널 열기 ----------------------
    public void OpenOptionsPanel()
    {
        StartCoroutine(OpenPanelAfterDelay());
    }

    private IEnumerator OpenPanelAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        optionsPanel.SetActive(true);
    }

    public void CloseOptionsPanel()
    {
        optionsPanel.SetActive(false);
    }
    // ---------------------- 게임 종료 ----------------------
    private void SetBtn(int index)
    {
        optionsPanel.SetActive(true);
    }
    // ---------------------- 게임 종료 ----------------------
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서는 Play모드 종료
#else
        Application.Quit(); // 빌드 실행 중일 때 종료
#endif
    }
}
