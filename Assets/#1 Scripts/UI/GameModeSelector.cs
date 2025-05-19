using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameModeSelector : MonoBehaviour
{
    public Image modeImage; // 모드 이미지
    public TMP_Text modeNameText; // 모드 이름 텍스트

    public Sprite[] modeSprites = new Sprite[3]; // 이미지 3장
    public string[] modeNames = { "게임1", "게임2", "게임3" }; // 모드 이름
    public string[] sceneNames = { "Scene1", "Scene2", "Scene3" }; // 대응되는 씬

    public Button leftButton;
    public Button rightButton;
    public Button startButton;
    public Button closeButton;

    private int currentMode = 0;

    private void Start()
    {
        leftButton.onClick.AddListener(SelectLeft);
        rightButton.onClick.AddListener(SelectRight);
        startButton.onClick.AddListener(StartSelectedMode);
        closeButton.onClick.AddListener(ClosePanel);

        UpdateDisplay();
    }

    private void SelectLeft()
    {
        currentMode = (currentMode - 1 + modeNames.Length) % modeNames.Length;
        UpdateDisplay();
    }

    private void SelectRight()
    {
        currentMode = (currentMode + 1) % modeNames.Length;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (modeSprites.Length > currentMode && modeImage != null)
            modeImage.sprite = modeSprites[currentMode];

        if (modeNameText != null)
            modeNameText.text = modeNames[currentMode];
    }

    private void StartSelectedMode()
    {
        SceneLoader.LoadScene(sceneNames[currentMode]);
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}