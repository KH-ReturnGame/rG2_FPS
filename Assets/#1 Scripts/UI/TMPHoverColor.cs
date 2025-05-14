using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMPHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text targetText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    private bool isHovering = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (targetText != null)
            targetText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (targetText != null)
            targetText.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 시에도 다시 색 적용
        if (!isHovering && targetText != null)
            targetText.color = normalColor;
    }
    
    private void OnDisable()
    {
        if (targetText != null)
            targetText.color = normalColor;
    }
}