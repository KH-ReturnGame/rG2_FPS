using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    public Slider comboSlider;
    public TextMeshProUGUI comboText; 
    public float comboSec = 5f; // 콤보 지속 시간

    private float timer = 0f; // 슬라이더 용 타이머
    private int comboCount = 0; // 연속 콤보 횟수
    private bool isComboActive = false;

    void Awake()
    {
		comboText.text = ""; // 연속 콤보 텍스트 초기화
    }

    void Update()
    {
        if (isComboActive)
        {
            timer -= Time.deltaTime;
            comboSlider.value = timer / comboSec;

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
            timer = comboSec;
        }
        else
        {
            comboCount++;
            timer = comboSec;
			
			// 콤보 2회 이상부터 -> 점수 x 콤보 수 만큼 점수 증가
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
        comboSlider.value = timer / comboSec;

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
