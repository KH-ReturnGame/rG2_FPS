using UnityEngine;

public class HeadMoving : MonoBehaviour
{
    [SerializeField]
    private Transform cameraHolder;

    [SerializeField]
    private float bobFrequency = 6f;
    [SerializeField]
    private float bobAmplitude = 0.025f;

    [SerializeField]
    private PlayerMovement movement;

    private Vector3 originalPosition;
    private float bobTimer = 0f;

    private void Start()
    {
        originalPosition = cameraHolder.localPosition;
    }

    private void Update()
    {
        bool isMoving = (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);

        if (movement != null && isMoving && !movement.IsCrouching)
        {
            bobTimer += Time.deltaTime * bobFrequency;  // MoveSpeed 곱하기 제거!

            float offsetY = Mathf.Sin(bobTimer) * bobAmplitude;

            cameraHolder.localPosition = originalPosition + new Vector3(0f, offsetY, 0f);
        }
        else
        {
            bobTimer = 0f;
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, originalPosition, Time.deltaTime * 8f);
        }
    }
}