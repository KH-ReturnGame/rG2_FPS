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
    
    [Header("Crouch Settings")]
    [SerializeField]
    private float crouchHeight = 1.0f;
    [SerializeField]
    private float standHeight = 2.0f;
    [SerializeField]
    private float crouchSpeed = 2.0f;
    [SerializeField]
    private float crouchTransitionSpeed = 10f;

    [SerializeField]
    private KeyCode crouchKey = KeyCode.C;

    private bool isCrouching = false;
    private bool crouchToggleState = false;
    
    private PlayerStatus status;
    
    public GameObject player;
    
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

    // Update is called once per frame
    void Update()
    {
        //1초당 moveForve 속력으로 이동
        characterController.Move(moveForce * Time.deltaTime);

        if(!characterController.isGrounded)
        {
            moveForce.y += gravity * Time.deltaTime;
        }
        
        HandleCrouch(); //안기 상태 처리
    }

    public void MoveTo(Vector3 direction)
    {
        // Compute world-space forward and right ignoring pitch
        Vector3 forward = player.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 rightDir = Vector3.Cross(Vector3.up, forward);

        // Determine flip based on camera pitch: flip when looking backward (up.y < 0)
        float flip = (player.transform.up.y < 0f) ? -1f : 1f;

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

        // 높이 전환
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
    }
}
