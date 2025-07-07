using UnityEngine;

public class HeadMoving : MonoBehaviour
{
    [SerializeField]
    private Transform cameraHolder; // 카메라 설정

    [SerializeField]
    private float bobFrequency;
    [SerializeField]
    private float bobAmplitude;

    [SerializeField]
    private PlayerMovement movement;

    [SerializeField]
    private WeaponAssaultRifle weapon;  // 조준 상태 확인용

    private Vector3 originalPosition;
    private float bobTimer = 0f;

    private void Start()
    {
        originalPosition = cameraHolder.localPosition;
    }

    private void Update()
    {
        bool isMoving = (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);

        if (movement != null && weapon != null && weapon.IsAiming && isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency;

            float crouchMultiplier = movement.IsCrouching ? 0.5f : 1f;

            float offsetY = Mathf.Sin(bobTimer) * bobAmplitude * crouchMultiplier;
            float offsetX = Mathf.Cos(bobTimer * 0.5f) * (bobAmplitude * 0.5f) * crouchMultiplier;

            cameraHolder.localPosition = originalPosition + new Vector3(offsetX, -offsetY, 0f);
        }
        else
        {
            bobTimer = 0f;
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, originalPosition, Time.deltaTime * 8f);
        }
    }
}