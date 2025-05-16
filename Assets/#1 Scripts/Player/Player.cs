using System;
using UnityEngine;
using System.Collections;

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
    [SerializeField] private AudioClip deathVoiceClip; // 🪦 사망 음성
    
    
    [SerializeField]
    public GameObject hitPanel;        // 피격 UI (계속 보여줄 것)
    public GameObject deathPanel;      // 선택: 사망용 UI 패널
    private AudioClip[] hitVoiceClips; // 피격 보이스들
    [SerializeField]
    private AudioSource voiceSource; // 별도의 보이스용 AudioSource

    public RotateToMouse rotateToMouse;    // 마우스 이동으로 카메라 회전
    private PlayerMovement movement;    // 키보드 입력으로 플레이어 이동, 점프
    private PlayerStatus status;        // 플레이어 정보
    private AudioSource audioSource;
    private WeaponBase weapon;
    
    // Track button states to handle rapid clicks
    private bool leftMouseWasDown = false;
    private bool rightMouseWasDown = false;
    
    private float lastHitVoiceTime = 0f;  // 마지막 피격음 재생 시각
    private float hitVoiceCooldown = 0.5f; // 최소 재생 간격 
    private bool hasPlayedDeathVoice = false; // 중복 방지

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

        //rotateToMouse = GetComponent<RotateToMouse>();
        movement = GetComponent<PlayerMovement>();
        status = GetComponent<PlayerStatus>();
        audioSource = GetComponent<AudioSource>();
        
        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
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
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f); // 🔧 감도 불러오기

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

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
            if (z > 0 && !movement.IsCrouching) isRun = Input.GetKey(keyCodeRun);

            if (!movement.IsCrouching)
            {
                movement.MoveSpeed = isRun ? status.RunSpeed : status.WalkSpeed;
            }
            else
            {
                movement.MoveSpeed = movement.CrouchSpeed;
            }
            weapon.Animator.MoveSpeed = isRun == true ? 1 : 0.5f;
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
            weapon.Animator.MoveSpeed = 0;

            if (audioSource.isPlaying == true) // 사운드 재생 중이면 정지
            {
                audioSource.Stop();
            }
        }
        
        float actualSpeed = new Vector3(movement.GetVelocity().x, 0, movement.GetVelocity().z).magnitude;
        float speedPercent = actualSpeed / status.RunSpeed;
        speedPercent = Mathf.Clamp01(speedPercent);

        // 볼륨, 속도 조정
        audioSource.volume = Mathf.Lerp(0.02f, 1.15f, speedPercent*0.5f);
        audioSource.pitch = Mathf.Lerp(0.5f, 1.15f, speedPercent);
        
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
        // Left mouse button (Fire)
        bool leftMouseDown = Input.GetMouseButton(0); // Changed to GetMouseButton for better detection
        
        if (leftMouseDown && !leftMouseWasDown)
        {
            // Mouse button was just pressed
            weapon.StartWeaponAction();
        }
        else if (!leftMouseDown && leftMouseWasDown)
        {
            // Mouse button was just released
            weapon.StopWeaponAction();
        }
        leftMouseWasDown = leftMouseDown;
        
        // Right mouse button (Aim)
        bool rightMouseDown = Input.GetMouseButton(1); // Changed to GetMouseButton for better detection
        
        if (rightMouseDown && !rightMouseWasDown)
        {
            // Mouse button was just pressed
            weapon.StartWeaponAction(1);
        }
        else if (!rightMouseDown && rightMouseWasDown)
        {
            // Mouse button was just released
            weapon.StopWeaponAction(1); // Fixed: Changed to StopWeaponAction instead of StartWeaponAction
        }
        rightMouseWasDown = rightMouseDown;

        // Reload
        if (Input.GetKeyDown(keyCodeReload))
        {
            weapon.StartReload();
        }
    }

    public void SwitchingWeapon(WeaponBase newWeapon)
    {
        weapon = newWeapon;
    }

    public void TakeDamage(int damage)
    {
        bool isDie = status.DecreaseHp(damage);

        if (isDie)
        {
            // 사망 음성 1회 재생
            if (!hasPlayedDeathVoice && deathVoiceClip != null)
            {
                voiceSource.PlayOneShot(deathVoiceClip);
                hasPlayedDeathVoice = true;
            }

            // 피격 UI 계속 표시
            if (hitPanel != null)
                hitPanel.SetActive(true);

            // 선택: 사망 전용 패널 표시
            if (deathPanel != null)
                deathPanel.SetActive(true);

            // 게임 정지
            Time.timeScale = 0f;

            Debug.Log("GameOver!");
        }
        else
        {
            // 일반 피격음
            PlayRandomHitVoice();

            // 피격 UI 잠시 표시
            if (hitPanel != null)
            {
                hitPanel.SetActive(true);
                StartCoroutine(HideHitPanelAfterDelay(0.5f));  // 짧게 보였다 사라짐
            }
        }
    }
    private void PlayRandomHitVoice()
    {
        // 쿨타임 검사
        if (Time.time - lastHitVoiceTime < hitVoiceCooldown)
            return;

        lastHitVoiceTime = Time.time;

        if (hitVoiceClips == null || hitVoiceClips.Length == 0) return;

        int index = UnityEngine.Random.Range(0, hitVoiceClips.Length);
        voiceSource.PlayOneShot(hitVoiceClips[index]);
    }
    
    private IEnumerator HideHitPanelAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Time.timeScale 무시
        if (status.CurrentHP > 0 && hitPanel != null)   // 죽은 상태이면 유지
            hitPanel.SetActive(false);
    }
}