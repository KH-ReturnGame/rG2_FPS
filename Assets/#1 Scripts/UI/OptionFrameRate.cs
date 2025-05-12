using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionFrameRate : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TMP_Dropdown frameRateDropdown;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private CanvasGroup frameDropdownCanvasGroup;

    private readonly int[] frameRates = { 30, 60, 120, -1 }; // 수정: 120FPS가 index 2

    private void Start()
    {
        // PlayerPrefs가 없을 경우 기본값 설정
        int savedFrameIndex = PlayerPrefs.HasKey("FrameRate") ? PlayerPrefs.GetInt("FrameRate") : 2; // 기본: 120FPS
        bool isVSyncOn = PlayerPrefs.HasKey("VSync") ? PlayerPrefs.GetInt("VSync") == 1 : true;      // 기본: VSync 켜짐

        // UI 적용
        frameRateDropdown.value = savedFrameIndex;
        frameRateDropdown.RefreshShownValue();
        vSyncToggle.isOn = isVSyncOn;

        // 설정 적용
        ApplySettings(savedFrameIndex, isVSyncOn);

        // 리스너 연결
        frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
        vSyncToggle.onValueChanged.AddListener(OnVSyncToggleChanged);
    }

    public void OnFrameRateChanged(int index)
    {
        PlayerPrefs.SetInt("FrameRate", index);
        ApplySettings(index, vSyncToggle.isOn);
    }

    public void OnVSyncToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("VSync", isOn ? 1 : 0);
        ApplySettings(frameRateDropdown.value, isOn);
    }

    private void ApplySettings(int frameIndex, bool vSyncOn)
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

        SetDropdownInteractable(!vSyncOn);

        Debug.Log($"[FrameSet] FrameRate: {Application.targetFrameRate}, VSync: {vSyncOn}");
    }

    private void SetDropdownInteractable(bool interactable)
    {
        frameRateDropdown.interactable = interactable;
        if (frameDropdownCanvasGroup != null)
        {
            frameDropdownCanvasGroup.alpha = interactable ? 1f : 0.5f;
        }
    }
}
