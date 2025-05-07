using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed; // �̵��ӵ�
    private Vector3 moveForce; // �̵� ��

    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float gravity; 
    
    [Header("���� ���� ����")]
    [SerializeField]
    private float crouchHeight = 1.0f; // ���� ���� ����
    [SerializeField]
    private float standHeight = 2.0f; // �Ϲ� ���� ����
    [SerializeField]
    private float crouchSpeed = 2.0f; // ���� ���� �ӵ�
    [SerializeField]
    private float crouchTransitionSpeed = 10f; // �ɱ� ��ȯ �ӵ�

    [SerializeField]
    private KeyCode crouchKey = KeyCode.C;

    private bool isCrouching = false;
    private bool crouchToggleState = false; // ��� ���� ����
    
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
        //1�ʴ� moveForve �ӷ����� �̵�
        characterController.Move(moveForce * Time.deltaTime);

        if(!characterController.isGrounded)
        {
            moveForce.y += gravity * Time.deltaTime;
        }
        
        HandleCrouch(); //�ɱ� ���� ó��
    }

    public void MoveTo(Vector3 direction)
    {
        // ī�޶��� pitch(���� ����)�� ������ ���� ���Ϳ� ���� ���� ���
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 rightDir = Vector3.Cross(Vector3.up, forward);

        // ī�޶� �������� �� �¿� ����
        float flip = (transform.up.y < 0f) ? -1f : 1f;
        
        // �Է� ������ ���� �������� ��ȯ
        direction = forward * direction.z + rightDir * direction.x;

        // �̵� �� = �̵����� * �ӵ�
        moveForce = new Vector3(direction.x * flip * moveSpeed, moveForce.y, direction.z * flip * moveSpeed);
    }

    public void Jump()
    {
        if(characterController.isGrounded)
        {
            moveForce.y = jumpForce;
        }
        
        // �����ϸ� �ɱ� ����
        isCrouching = false;
        crouchToggleState = false;

    }
    
    private void HandleCrouch()
    {
        //C Ű ������ ���� ����
        if (Input.GetKeyDown(crouchKey))
        {
            crouchToggleState = !crouchToggleState;
            isCrouching = crouchToggleState;
        }

        // ĳ���� ���� �ε巴�� ����
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
    }
}
