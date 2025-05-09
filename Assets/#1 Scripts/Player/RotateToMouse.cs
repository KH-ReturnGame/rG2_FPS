using UnityEngine;
using UnityEngine.UI; // UI.Image, RectTransform 사용을 위해 필요

public class RotateToMouse : MonoBehaviour
{
    [Header("Camera Rotation")]
    [SerializeField] private float rotCamXAxisSpeed = 5f;
    [SerializeField] private float rotCamYAxisSpeed = 3f;
    [SerializeField] private float limitMinX = -90f;
    [SerializeField] private float limitMaxX = 80f;

    [Header("Lean Movement Settings")]
    [SerializeField] private float leanRadius = 0.2f;
    [SerializeField] private float leanDuration = 0.15f;

    [Header("UI Aim Offset")]
    [SerializeField] private RectTransform aimUI;
    [SerializeField] private float maxAimOffsetX = 30f;
    [SerializeField] private float maxAimOffsetY = 20f;

    public float targetOffset = 10f;

    private float eulerAngleX;
    private float eulerAngleY;

    private float leanTimer = 0f;
    private int leanDirection = 0;

    private Vector3 defaultLocalPos;
    private Vector3 currentOffset = Vector3.zero;
    private Vector3 leanVelocity = Vector3.zero;
    private Vector2 baseAimPos;

    private void Start()
    {
        defaultLocalPos = transform.localPosition;
        if (aimUI != null)
            baseAimPos = aimUI.anchoredPosition;
    }

    public void UpdateRotate(float mouseX, float mouseY)
    {
        eulerAngleY += mouseX * rotCamXAxisSpeed;
        eulerAngleX -= mouseY * rotCamYAxisSpeed;
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX + targetOffset, limitMaxX + targetOffset);

        transform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0f);

        UpdateLeanPosition();
    }

    private float ClampAngle(float angle, float min, float max)
    {
        return Mathf.Clamp(angle, min, max);
    }

    private void UpdateLeanPosition()
    {
        if (Input.GetKey(KeyCode.E)) leanDirection = 1;
        else if (Input.GetKey(KeyCode.Q)) leanDirection = -1;
        else leanDirection = 0;

        if (leanDirection != 0)
            leanTimer = Mathf.Clamp01(leanTimer + Time.deltaTime / leanDuration);
        else
            leanTimer = Mathf.Clamp01(leanTimer - Time.deltaTime / leanDuration);

        float angle = Mathf.Lerp(0, Mathf.PI / 2, leanTimer);
        float xOffset = leanRadius * Mathf.Sin(angle) * leanDirection;
        float yOffset = -leanRadius * Mathf.Sin(angle);

        currentOffset = new Vector3(xOffset, yOffset, 0f);
        transform.localPosition = defaultLocalPos + currentOffset;

        UpdateAimUI(angle); // ← 이 줄 추가!
    }

    private void UpdateAimUI(float angle)
    {
        if (aimUI == null) return;

        float offsetX = Mathf.Sin(angle) * maxAimOffsetX * leanDirection;
        float offsetY = -Mathf.Sin(angle) * maxAimOffsetY;
        aimUI.anchoredPosition = baseAimPos + new Vector2(offsetX, offsetY);
    }
}
