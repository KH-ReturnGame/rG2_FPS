using UnityEngine;

public class PlayerAnimateController : MonoBehaviour
{
    private Animator animator;

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

    /// Assaultt Rifle 마우스 오른쪽 클릭 액션
    public bool AimModeIs
    {
        set => animator.SetBool("isAimMode", value);
        get => animator.GetBool("isAimMode");
    }

    public void SetFloat(string paramName, float value)
    {
        animator.SetFloat(paramName, value);
    }
}
