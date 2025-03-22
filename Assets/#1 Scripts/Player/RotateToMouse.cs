using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 5; // 카메라 x회전 감도

    [SerializeField]
    private float rotCamYAxisSpeed = 3; // 카메라 y회전 감도

    private float limitMinX = -80; // 카메라 x 회전범위
    private float limitMaxX = 50; // 카메라y  회전범위
    private float eulerAngleX;
    private float eulerAngleY;

    // 마우스 움직임에 따라 회전 처리
    public void UpdateRotate(float mouseX,float mouseY)
    {
        eulerAngleY += mouseX * rotCamXAxisSpeed; // 마우스 좌/우 이동으로 y회전
        eulerAngleX -= mouseY * rotCamXAxisSpeed; // 마우스 상/하 이동으로 x회전

        // 카메라 x회전 범위 제한
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);

        // 회전 적용
        transform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0);
    }

    // 특정 범위 내로 회전 값을 제한하는 함수
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }
}
