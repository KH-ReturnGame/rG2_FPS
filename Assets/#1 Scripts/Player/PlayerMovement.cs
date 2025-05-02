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

    public float MoveSpeed
    {
        set => moveSpeed = Mathf.Max(0, value);
        get => moveSpeed;
    }

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //1�ʴ� moveForve �ӷ����� �̵�
        characterController.Move(moveForce * Time.deltaTime);

        if(!characterController.isGrounded)
        {
            moveForce.y += gravity * Time.deltaTime;
        }
    }

    public void MoveTo(Vector3 direction)
    {
        // Compute world-space forward and right ignoring pitch
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 rightDir = Vector3.Cross(Vector3.up, forward);

        // Determine flip based on camera pitch: flip when looking backward (up.y < 0)
        float flip = (transform.up.y < 0f) ? -1f : 1f;

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
    }
}
