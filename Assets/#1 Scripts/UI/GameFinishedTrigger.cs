using UnityEngine;

public class GameFinishedTrigger : MonoBehaviour
{
    [Header("완료 패널 연결")]
    public GameFinishedUI gameFinishedPanel;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 완료 구역에 들어오면 패널 표시
        if (other.CompareTag("Player"))
        {
            if (gameFinishedPanel != null)
            {
                gameFinishedPanel.ShowGameFinishedUI();
            }
            else
            {
                Debug.LogError("GameFinishedPanel이 연결되지 않았습니다!");
            }
        }
    }
}