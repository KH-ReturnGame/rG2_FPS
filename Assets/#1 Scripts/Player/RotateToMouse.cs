using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 5; // ī�޶� xȸ�� ����

    [SerializeField]
    private float rotCamYAxisSpeed = 3; // ī�޶� yȸ�� ����

    private float limitMinX = -90; // ī�޶� x ȸ������
    private float limitMaxX = 80; // ī�޶�y  ȸ������
    private float eulerAngleX;
    private float eulerAngleY;

    public float targetOffset = 10;

    // ���콺 �����ӿ� ���� ȸ�� ó��
    public void UpdateRotate(float mouseX,float mouseY)
    {
        eulerAngleY += mouseX * rotCamXAxisSpeed; // ���콺 ��/�� �̵����� yȸ��
        eulerAngleX -= mouseY * rotCamXAxisSpeed; // ���콺 ��/�� �̵����� xȸ��

        // ī�޶� xȸ�� ���� ����
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX+targetOffset, limitMaxX+targetOffset);

        // ȸ�� ����
        transform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0);
    }

    // Ư�� ���� ���� ȸ�� ���� �����ϴ� �Լ�
    private float ClampAngle(float angle, float min, float max)
    {
        // Directly clamp without wrapping to avoid sudden jumps at zero crossing
        return Mathf.Clamp(angle, min, max);
    }
}
