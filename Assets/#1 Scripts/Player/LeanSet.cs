using UnityEngine;
using UnityEngine.UI;

public class LeanSet : MonoBehaviour
{
    [Header("Lean Settings")]
    [SerializeField] private float leanDuration = 0.15f;
    [SerializeField] private float cameraRollAngle = 10f;
    [SerializeField] private float leanRadius = 0.2f;

    [Header("Camera & UI")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private RectTransform aimUI;
    [SerializeField] private float maxAimOffsetX = 30f;
    [SerializeField] private float maxAimOffsetY = 20f;

    private float leanTimer = 0f;
    private float targetLean = 0f;
    private float leanVelocity = 0f;

    private Vector2 baseAimPos;
    private Vector3 defaultLocalPos;
    private Vector3 camOrigin;

    public float CurrentLeanRatio => leanTimer;

    private void Start()
    {
        defaultLocalPos = transform.localPosition;

        if (cameraTransform != null)
            camOrigin = cameraTransform.localPosition;

        if (aimUI != null)
            baseAimPos = aimUI.anchoredPosition;
    }

    private void Update()
    {
        HandleLean();
    }

    private void HandleLean()
    {
        // 입력 처리
        if (Input.GetKey(KeyCode.Q)) targetLean = 1f;
        else if (Input.GetKey(KeyCode.E)) targetLean = -1f;
        else targetLean = 0f;

        // 보간
        leanTimer = Mathf.SmoothDamp(leanTimer, targetLean, ref leanVelocity, leanDuration);

        // 반원 궤적 계산
        float angle = Mathf.Abs(leanTimer) * Mathf.PI / 2f;
        float xOffset = Mathf.Sin(angle) * leanRadius * Mathf.Sign(leanTimer);
        float yOffset = -Mathf.Sin(angle) * leanRadius * 0.3f;

        // ✅ 방향 기준 이동 (카메라 회전 반영)
        Vector3 offset = -transform.right * xOffset - transform.up * yOffset;
        transform.localPosition = defaultLocalPos + offset;

        // 카메라 Z축 회전
        if (cameraTransform != null)
        {
            float roll = cameraRollAngle * leanTimer;
            cameraTransform.localRotation = Quaternion.Euler(0f, 0f, roll);
        }

        UpdateAimUI(offset);
    }

    private void UpdateAimUI(Vector3 offset)
    {
        if (aimUI == null) return;

        float offsetX = Vector3.Dot(offset.normalized, transform.right) * maxAimOffsetX;
        float offsetY = Vector3.Dot(offset.normalized, transform.up) * maxAimOffsetY;

        aimUI.anchoredPosition = baseAimPos + new Vector2(offsetX, offsetY);
    }
}
