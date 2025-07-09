using UnityEngine;

namespace ScansFactory
{
    [RequireComponent(typeof(CharacterController))]
    public class SF_FPSController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float WalkSpeed;
        public float RunSpeed;
        public float FlySpeed;
        public float JumpSpeed;
        public float Gravity;

        [Header("Mouse Look Settings")]
        public float LookSpeed;
        public float LookXLimit;

        [Header("Extra")]
        public GameObject Flashlight;
        

        private CharacterController characterController;
        private Vector3 moveDirection = Vector3.zero;
        private float rotationX = 0;
        private bool noClip = false;


        void Start()
        {
            characterController = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.CapsLock))
            {
                noClip = !noClip;
                characterController.enabled = !noClip;
            }
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                Flashlight.SetActive(!Flashlight.activeSelf);
            }

            MouseLook();

            if (noClip)
                FlyMovement();
            else
                Movement();
        }

        void Movement()
        {
            float speed = Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed;
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");

            Vector3 move = transform.right * inputX + transform.forward * inputY;
            move = move.normalized * speed;

            if (characterController.isGrounded)
            {
                moveDirection = move;
                if (Input.GetKeyDown(KeyCode.Space))
                    moveDirection.y = JumpSpeed;
                else
                    moveDirection.y = -Gravity * Time.deltaTime;
            }
            else
            {
                moveDirection.x = move.x;
                moveDirection.z = move.z;
                moveDirection.y -= Gravity * Time.deltaTime;
            }

            characterController.Move(moveDirection * Time.deltaTime);
        }

        void FlyMovement()
        {
            float speed = FlySpeed * Time.deltaTime;
            float inputX = Input.GetAxis("Horizontal") * speed;
            float inputY = Input.GetAxis("Vertical") * speed;
            float inputZ = 0;

            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E))
                inputZ = speed;
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q))
                inputZ = -speed;

            Vector3 move = transform.right * inputX + transform.forward * inputY + transform.up * inputZ;
            transform.position += move;
        }

        void MouseLook()
        {
            rotationX += -Input.GetAxis("Mouse Y") * LookSpeed;
            rotationX = Mathf.Clamp(rotationX, -LookXLimit, LookXLimit);
            Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * LookSpeed, 0);
        }
    }
}