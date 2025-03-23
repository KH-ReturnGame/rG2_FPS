using UnityEngine;

public class PlayerAnimateController : MonoBehaviour
{
    private Animator animator;// �ִϸ����� ������Ʈ

    private void Awake()
    {
        // �ڽ� ������Ʈ���� �ִϸ����� ��������
        animator = GetComponentInChildren<Animator>();
    }

    // �̵� �ӵ��� �ִϸ������� �Ķ���� ������ ����
    public float MoveSpeed
    {
        set => animator.SetFloat("movementSpeed", value);
        get => animator.GetFloat("movementSpeed");
    }

    public void Play(string stateName, int layer, float normalizedTime)
    {
        animator.Play(stateName, layer, normalizedTime);
    }
}
