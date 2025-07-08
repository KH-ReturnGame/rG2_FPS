using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [Header("플레이어 트랜스폼")]
    public Transform player;

    [Header("카메라 높이 고정")]
    public float fixedHeight = 50f;

    [Header("회전 동기화 여부")]
    public bool rotateWithPlayer = true;

    private void LateUpdate()
    {
        // 플레이어 위치 따라가기
        Vector3 newPosition = player.position;
        newPosition.y = fixedHeight;
        transform.position = newPosition;

        // 회전 동기화
        if (rotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f); // 고정
        }
    }
}
