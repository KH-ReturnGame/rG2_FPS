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
        //1초당 moveForve 속력으로 이동
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

        // 이동 힘 = 이동방향 * 속도
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
