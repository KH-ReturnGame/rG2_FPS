using UnityEngine;

[System.Serializable]  //(1) 구조체를 인스펙터에서 볼 수 있도록 설정
public struct WeaponSet // (2) 구조체 정의 (클래스랑 비슷하지만 가벼움)
{
    public float attakRate; // 공격 속도
    public float attakDistance; // 공격  사거리
    public bool isAutomaticAttack; // 연속 공격 여부
}