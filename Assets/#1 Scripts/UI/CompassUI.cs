using UnityEngine;
using UnityEngine.UI;

public class CompassUI : MonoBehaviour
{
    // 화면 위쪽 나침반 UI
    public RawImage compassImage;
    public Transform playerCamera;

    void Update()
    {
        float yaw = playerCamera.eulerAngles.y;
        float normalizedYaw = yaw / 360f;
        compassImage.uvRect = new Rect(normalizedYaw, 0, 1, 1); // 좌우 이동
    }
}
