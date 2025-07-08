using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    public Slider comboSlider;
    public TextMeshProUGUI comboText; 
    public float comboWindow = 5f; // 콤보 지속 시간

    private float timer = 0f; 
    private int comboCount = 0; // 연속 콤보 횟수
    private bool isComboActive = false;

    void Awake()
    {
		comboText.text = "";
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (isComboActive)
        {
            timer -= Time.deltaTime;
            comboSlider.value = timer / comboWindow;

            if (timer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public void RegisterKill()
    {
        if (!isComboActive)
        {
            isComboActive = true;
            comboCount = 1;
            timer = comboWindow;
        }
        else
        {
            comboCount++;
            timer = comboWindow;

            if (comboCount >= 2)
            {
                int bonus = (comboCount - 1) * 100;
                ScoreManager.Instance.AddScore(bonus);
            }
        }

        UpdateUI();
    }

    void ResetCombo()
    {
        isComboActive = false;
        comboCount = 0;
        timer = 0f;
        comboSlider.value = 0;
        comboText.text = "";
    }

    void UpdateUI()
    {
        comboSlider.gameObject.SetActive(true);
        comboSlider.value = timer / comboWindow;

        // 연속처치 텍스트
        if (comboCount >= 2)
        {
            comboText.text = $"연속처치 x{comboCount}";
        }
        else
        {
            comboText.text = "";
        }
    }
}
