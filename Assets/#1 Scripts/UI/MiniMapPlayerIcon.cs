using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MiniMapPlayerIcon : MonoBehaviour
{
    [Header("플레이어 오브젝트")]
    public Transform player;

    [Header("미니맵 전용 카메라 (Orthographic)")]
    public Camera miniMapCamera;

    [Header("마우스 회전 기준 (예: 마우스로 회전하는 카메라 등)")]
    public Transform aimReference;

    private RectTransform rectTransform;
    private RectTransform parentRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = transform.parent.GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (player == null || miniMapCamera == null || aimReference == null) return;

        // 1. 위치 계산
        Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(player.position);
        Vector2 size = parentRect.rect.size;

        Vector2 localPos = new Vector2(
            (viewportPos.x - 0.5f) * size.x,
            (viewportPos.y - 0.5f) * size.y
        );

        rectTransform.localPosition = localPos;

        // 2. 마우스 회전 기준으로 아이콘 회전
        float angleY = aimReference.eulerAngles.y;
        rectTransform.rotation = Quaternion.Euler(0f, 0f, -angleY);
    }
}