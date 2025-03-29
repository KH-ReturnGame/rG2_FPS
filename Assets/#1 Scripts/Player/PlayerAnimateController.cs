using UnityEngine;

public class PlayerAnimateController : MonoBehaviour
{
    private Animator animator;// 애니메이터 컴포넌트

    private void Awake()
    {
        // 자식 오브젝트에서 애니메이터 가져오기
        animator = GetComponentInChildren<Animator>();
    }

    public void OnReload()
    {
        animator.SetTrigger("onReload");
    }

    public bool CurrentAnimationIs(string name)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    // 이동 속도를 애니메이터의 파라미터 값으로 설정
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
