// ������ ������ ���� ������ �� �������� ����ϴ� �������� ����ü�� ��� �����ϸ�
// ������ �߰�/������ �� ����ü�� �����ϱ� ������ ������ ������

public enum WeaponName
{
    AssaultRifle = 0,
}

[System.Serializable]  //(1) ����ü�� �ν����Ϳ��� �� �� �ֵ��� ����
public struct WeaponSet // (2) ����ü ���� (Ŭ������ ��������� ������)
{
    public WeaponName weaponName; //���� �̸�
    public int damage;
    public int currentAmmo; // ���� ź�� ��
    public int maxAmmo; // �ִ� ź�� ��
    public float attackRate; // ���� �ӵ�
    public float attackDistance; // ���� ��Ÿ�
    public bool isAutomaticAttack; // ���� ���� ����
}