using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 5; // ī�޶� xȸ�� ����

    [SerializeField]
    private float rotCamYAxisSpeed = 3; // ī�޶� yȸ�� ����
    
    [SerializeField]
    private float leanAngle = 15f; // �ִ� ����̱� ����
    [SerializeField]
    private float leanSpeed = 5f; // ����̱� �ӵ�
    
    private float currentLean = 0f; // ���� ����� ����
    private float leanVelocity = 0f;

    private float limitMinX = -90; // ī�޶� x ȸ������
    private float limitMaxX = 80; // ī�޶�y  ȸ������
    private float eulerAngleX;
    private float eulerAngleY;

    public float targetOffset = 10;
    //public GameObject Player;

    // ���콺 �����ӿ� ���� ȸ�� ó��
    public void UpdateRotate(float mouseX,float mouseY)
    {
        eulerAngleY += mouseX * rotCamXAxisSpeed; // ���콺 ��/�� �̵����� yȸ��
        eulerAngleX -= mouseY * rotCamXAxisSpeed; // ���콺 ��/�� �̵����� xȸ��

        // ī�޶� xȸ�� ���� ����
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX+targetOffset, limitMaxX+targetOffset);
        
        UpdateLean(); // ���� ������Ʈ
        
        // ȸ�� ����
        transform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, currentLean);
    }

    // Ư�� ���� ���� ȸ�� ���� �����ϴ� �Լ�
    private float ClampAngle(float angle, float min, float max)
    {
        // Directly clamp without wrapping to avoid sudden jumps at zero crossing
        return Mathf.Clamp(angle, min, max);
    }
    
    private void UpdateLean()
    {
        //Debug.Log("currentLean : " + currentLean);
        float targetLean = 0f;
        // Q ����, E ������
        if (Input.GetKey(KeyCode.E))
        {
            targetLean = -leanAngle;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            targetLean = leanAngle;
        }
        
        // �ε巴�� ���� ���� ����
        currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, 0.05f);
    }
    
}
