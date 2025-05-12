using UnityEngine;
using TMPro;

public class ScreenModeOption: MonoBehaviour
{
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    private void Start()
    {
        int savedIndex = PlayerPrefs.HasKey("ScreenMode") ? PlayerPrefs.GetInt("ScreenMode") : 0; // 기본값: 전체 화면

        screenModeDropdown.value = savedIndex;
        screenModeDropdown.RefreshShownValue();
        ApplyScreenMode(savedIndex);

        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
    }

    public void OnScreenModeChanged(int index)
    {
        PlayerPrefs.SetInt("ScreenMode", index);
        ApplyScreenMode(index);
    }

    private void ApplyScreenMode(int index)
    {
        switch (index)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            default:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
        }

        Debug.Log($"[ScreenModeSet] 현재 화면 모드: {Screen.fullScreenMode}");
    }
}