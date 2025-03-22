using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 5; // ī�޶� xȸ�� ����

    [SerializeField]
    private float rotCamYAxisSpeed = 3; // ī�޶� yȸ�� ����

    private float limitMinX = -80; // ī�޶� x ȸ������
    private float limitMaxX = 50; // ī�޶�y  ȸ������
    private float eulerAngleX;
    private float eulerAngleY;

    // ���콺 �����ӿ� ���� ȸ�� ó��
    public void UpdateRotate(float mouseX,float mouseY)
    {
        eulerAngleY += mouseX * rotCamXAxisSpeed; // ���콺 ��/�� �̵����� yȸ��
        eulerAngleX -= mouseY * rotCamXAxisSpeed; // ���콺 ��/�� �̵����� xȸ��

        // ī�޶� xȸ�� ���� ����
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);

        // ȸ�� ����
        transform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0);
    }

    // Ư�� ���� ���� ȸ�� ���� �����ϴ� �Լ�
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }
}
