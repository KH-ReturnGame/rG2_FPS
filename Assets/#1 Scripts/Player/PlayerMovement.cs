using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed; // 이동속도
    private Vector3 moveForce; // 이동 힘

    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float gravity; 
    
    [Header("앉은 상태 변수")]
    [SerializeField]
    private float crouchHeight = 1.0f; // 앉은 상태 높이
    [SerializeField]
    private float standHeight = 2.0f; // 일반 상태 높이
    [SerializeField]
    private float crouchSpeed = 2.0f; // 앉은 상태 속도
    [SerializeField]
    private float crouchTransitionSpeed = 10f; // 앉기 전환 속도

    [SerializeField]
    private KeyCode crouchKey = KeyCode.C;

    private bool isCrouching = false;
    private bool crouchToggleState = false; // 토글 상태 변수
    
    private PlayerStatus status;
    public float MoveSpeed
    {
        set => moveSpeed = Mathf.Max(0, value);
        get => moveSpeed;
    }
    
    public float CrouchSpeed => crouchSpeed;
    public bool IsCrouching => isCrouching;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        //1초당 moveForve 속력으로 이동
        characterController.Move(moveForce * Time.deltaTime);

        if(!characterController.isGrounded)
        {
            moveForce.y += gravity * Time.deltaTime;
        }
        
        HandleCrouch(); //앉기 상태 처리
    }

    public void MoveTo(Vector3 direction)
    {
        // 카메라의 pitch(상하 기울기)를 무시한 전방 벡터와 우측 벡터 계산
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 rightDir = Vector3.Cross(Vector3.up, forward);

        // 카메라가 뒤집혔을 때 좌우 반전
        float flip = (transform.up.y < 0f) ? -1f : 1f;
        
        // 입력 방향을 월드 방향으로 변환
        direction = forward * direction.z + rightDir * direction.x;

        // 이동 힘 = 이동방향 * 속도
        moveForce = new Vector3(direction.x * flip * moveSpeed, moveForce.y, direction.z * flip * moveSpeed);
    }

    public void Jump()
    {
        if(characterController.isGrounded)
        {
            moveForce.y = jumpForce;
        }
        
        // 점프하면 앉기 해제
        isCrouching = false;
        crouchToggleState = false;

    }
    
    private void HandleCrouch()
    {
        //C 키 누르면 상태 변경
        if (Input.GetKeyDown(crouchKey))
        {
            crouchToggleState = !crouchToggleState;
            isCrouching = crouchToggleState;
        }

        // 캐릭터 높이 부드럽게 변경
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
    }
}
