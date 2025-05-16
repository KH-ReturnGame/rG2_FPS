using UnityEngine;

public class WeaponLeaner : MonoBehaviour
{
    [Header("Lean 설정")]
    [SerializeField] private float leanAngle = 15f;
    [SerializeField] private float leanSmooth = 0.05f;

    [Header("카메라 이동 설정")]
    [SerializeField] private Transform cameraTransform; // Main Camera 참조
    [SerializeField] private float cameraLeanOffsetX = 0.05f;
    [SerializeField] private float cameraLeanOffsetY = 0.025f;

    private float currentLean = 0f;
    private float leanVelocity = 0f;

    private float camVelocityX = 0f;
    private float camVelocityY = 0f;
    private Vector3 originalCamLocalPos;

    void Start()
    {
        if (cameraTransform != null)
            originalCamLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        // 1. Lean 방향 감지
        float targetLean = 0f;
        if (Input.GetKey(KeyCode.Q)) targetLean = leanAngle;
        else if (Input.GetKey(KeyCode.E)) targetLean = -leanAngle;

        // 2. 총기 Z축 기울이기
        currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSmooth);
        transform.localRotation = Quaternion.Euler(0f, 0f, currentLean); // 중앙 유지

        // 3. Lean 비율 계산
        float leanRatio = currentLean / leanAngle; // -1 ~ 1

        // 4. 카메라 이동 (Lean 방향 따라 X + 위로 Y)
        float targetX = -leanRatio * cameraLeanOffsetX; // ← 방향 반전해서 동일 방향 이동
        float targetY = Mathf.Abs(leanRatio) * cameraLeanOffsetY;

        float newX = Mathf.SmoothDamp(cameraTransform.localPosition.x, originalCamLocalPos.x + targetX, ref camVelocityX, 0.05f);
        float newY = Mathf.SmoothDamp(cameraTransform.localPosition.y, originalCamLocalPos.y + targetY, ref camVelocityY, 0.05f);

        cameraTransform.localPosition = new Vector3(newX, newY, originalCamLocalPos.z);
    }
}