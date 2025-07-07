using UnityEngine;
using UnityEngine.UI;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    public Slider comboSlider;
    public float comboSec = 5f; // 연속 킬 유지 시간
    private float timer = 0f;

    private int comboCount = 0;
    private bool isComboActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

            if (comboCount >= 2)
            {
                int bonus = (comboCount - 1) * 100; // 2킬부터 보너스 100씩 증가
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
    }

    void UpdateUI()
    {
        if (comboSlider != null)
        {
            comboSlider.gameObject.SetActive(true);
            comboSlider.value = timer / comboSec;
        }
    }
}