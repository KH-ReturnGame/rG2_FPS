using UnityEngine;

[System.Serializable]  //(1) ����ü�� �ν����Ϳ��� �� �� �ֵ��� ����
public struct WeaponSet // (2) ����ü ���� (Ŭ������ ��������� ������)
{
    public float attakRate; // ���� �ӵ�
    public float attakDistance; // ����  ��Ÿ�
    public bool isAutomaticAttack; // ���� ���� ����
}