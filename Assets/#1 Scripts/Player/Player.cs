using System;
using UnityEngine;

public enum PlayerStats
{
    IsGround = 0,
}

public class Player : MonoBehaviour
{
    //플레이어가 가질 수 있는 모든 상태 개수
    private static readonly int StateCount = Enum.GetValues(typeof(PlayerStats)).Length;
    //플레이어가 가질 수 있는 모든 상태들 배열
    private State<Player>[] _states;
    private StateManager<Player> _stateManager;

    // _________________________________________________________

    [Header("Input KeyCodes")]
    [SerializeField]
    private KeyCode keyCodeRun = KeyCode.LeftShift; // 달리기 키 설정
    [SerializeField]
    private KeyCode keyCodeJump = KeyCode.Space; // 달리기 키 설정
    [SerializeField]
    private KeyCode keyCodeReload = KeyCode.R; // 재장전 키 설정
    
    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipWalk; // 걷기 사운드
    [SerializeField]
    private AudioClip audioClipRun; // 걷기 사운드

    private RotateToMouse rotateToMouse;    // 마우스 이동으로 카메라 회전
    private PlayerMovement movement;    // 키보드 입력으로 플레이어 이동, 점프
    private PlayerStatus status;        // 플레이어 정보
    private PlayerAnimateController animator; // 애니메이션 재생 제어
    private AudioSource audioSource;
    private WeaponAssaultRifle weapon;



    public void Start()
    {
        //_states 초기화
        _states = new State<Player>[StateCount];
        _states[(int)PlayerStats.IsGround] = new PlayerOwnedStates.IsGround();
        
        //상태 관리자 정의
        _stateManager = new StateManager<Player>();
        _stateManager.Setup(this,StateCount,_states);
    }

    private void Awake()
    {
        // 마우스 보이지 않게 설정/ 현재 위치에 고정
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        rotateToMouse = GetComponent<RotateToMouse>();
        movement = GetComponent<PlayerMovement>();
        status = GetComponent<PlayerStatus>();
        animator = GetComponent<PlayerAnimateController>();
        audioSource = GetComponent<AudioSource>();
        weapon = GetComponentInChildren<WeaponAssaultRifle>();
    }
    public void Update()
    {
        //상태 매니저의 Execute실행
        _stateManager.Execute();

        // 마우스 회전,이동 업데이트
        UpdateRotate();
        UpdateMove();
        UpdateJump();
        UpdateWeaponAction();
    }
    
    //상태 추가 메소드
    public void AddState(PlayerStats ps)
    {
        State<Player> newState = _states[(int)ps];
        _stateManager.AddState(newState);
    }
    
    //상태 제거 메소드
    public void RemoveState(PlayerStats ps)
    {
        State<Player> remState = _states[(int)ps];
        _stateManager.RemoveState(remState);
    }
    //상태 있는지 체크
    public bool IsContainState(PlayerStats ps)
    {
        return _stateManager._currentState.Contains(_states[(int)ps]);
    }


    private void UpdateRotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }

    private void UpdateMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // 이동중일 때 ( 걷기 , 뛰기)
        if (x != 0 || z != 0)
        {
            bool isRun = false;

            // 옆, 뒤 이동할 때 달리기 제한
            if (z > 0) isRun = Input.GetKey(keyCodeRun);

            movement.MoveSpeed = isRun == true ? status.RunSpeed : status.WalkSpeed;
            animator.MoveSpeed = isRun == true ? 1 : 0.5f;
            audioSource.clip = isRun == true ? audioClipRun : audioClipWalk;

            // 오디오가 재생 중이 아닐 때만 재생
            if (audioSource.isPlaying == false)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        // 제자리에 멈춰 있을 때
        else
        {
            movement.MoveSpeed = 0;
            animator.MoveSpeed = 0;

            if (audioSource.isPlaying == true) // 사운드 재생 중이면 정지
            {
                audioSource.Stop();
            }
        }

        movement.MoveTo(new Vector3(x, 0, z));
    }

    private void UpdateJump()
    {
        if (Input.GetKeyDown(keyCodeJump))
        {
            movement.Jump();
        }
    }

    private void UpdateWeaponAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            weapon.StartWeaponAction();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            weapon.StopWeaponAction();
        } 

        if (Input.GetMouseButtonDown(1))
        {
            weapon.StartWeaponAction(1);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            weapon.StartWeaponAction(1);
        }

        if (Input.GetKeyDown(keyCodeReload))
        {
            weapon.StartReload();
        }
    }
}

