using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 스피드런 같은 거 신경 안 쓰고 바로 게임을 시작하는 코드
    private void Start()
    {
        Debug.Log("게임 시작! 메인 로직 가동");
        
        // 만약 시작하자마자 초기화해야 할 게 있다면 여기에 작성하세요.
        // 예: Player.Init(); 
    }

    // 만약 화면에 시간을 꼭 표시해야 하는 게 아니라면 Update도 비워둬도 됩니다.
}