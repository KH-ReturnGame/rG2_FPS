using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //플레이어가 들어가면 발동되는 체크포인트
    public GameObject[] CheckPoints;

    //체크포인트 내부로 들어갔을 때 발동되는 이벤트
    public event EventHandler OnTrrigerCP;
}
