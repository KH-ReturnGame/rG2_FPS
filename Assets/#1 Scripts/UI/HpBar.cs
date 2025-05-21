using UnityEngine;
using UnityEngine.UI;
public class HpBar : MonoBehaviour
{
    [SerializeField] private Image barFillImage;  // fill 이미지
    [SerializeField] private PlayerStatus playerStatus;

    private void Start()
    {
        if (playerStatus != null)
            playerStatus.onHpEvent.AddListener(OnHpChanged);
    }

    private void OnDestroy()
    {
        if (playerStatus != null)
            playerStatus.onHpEvent.RemoveListener(OnHpChanged);
    }

    private void OnHpChanged(float prevHp, float currHp)
    {
        float hpPercent = currHp / playerStatus.MaxHp;
        UpdateBar(hpPercent);
    }

    private void UpdateBar(float percent)
    {
        if (barFillImage != null)
        {
            barFillImage.fillAmount = percent;
            barFillImage.color = Color.Lerp(Color.red, Color.white, percent);
        }
    }
}
