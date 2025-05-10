using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [Header("Camera Rotation")]
    [SerializeField] private float rotCamXAxisSpeed = 5f;
    [SerializeField] private float rotCamYAxisSpeed = 3f;
    [SerializeField] private float limitMinX = -90f;
    [SerializeField] private float limitMaxX = 80f;

    public float targetOffset = 10f;

    private float eulerAngleX; // pitch
    private float eulerAngleY; // yaw

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        UpdateRotate(mouseX, mouseY);
    }

    public void UpdateRotate(float mouseX, float mouseY)
    {
        eulerAngleY += mouseX * rotCamXAxisSpeed;
        eulerAngleX -= mouseY * rotCamYAxisSpeed;

        eulerAngleX = ClampAngle(eulerAngleX, limitMinX + targetOffset, limitMaxX + targetOffset);

        // ✅ 반전 문제 없이 회전 적용 (Yaw * Pitch 순으로)
        Quaternion yawRotation = Quaternion.AngleAxis(eulerAngleY, Vector3.up);
        Quaternion pitchRotation = Quaternion.AngleAxis(eulerAngleX, Vector3.right);
        transform.rotation = yawRotation * pitchRotation;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        return Mathf.Clamp(angle, min, max);
    }
}