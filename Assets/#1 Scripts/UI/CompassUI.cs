using UnityEngine;
using UnityEngine.UI;

public class CompassUI : MonoBehaviour
{
    public RawImage compassImage;
    public Transform playerCamera;

    void Update()
    {
        float yaw = playerCamera.eulerAngles.y;
        float normalizedYaw = yaw / 360f;
        compassImage.uvRect = new Rect(normalizedYaw, 0, 1, 1); // 좌우 이동
    }
}
