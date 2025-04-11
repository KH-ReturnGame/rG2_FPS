using System.Collections;
using Unity.Mathematics.Geometry;
using UnityEngine;

[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }
[System.Serializable]
public class MagazineEvent : UnityEngine.Events.UnityEvent<int> { }


public class WeaponAssaultRifle : MonoBehaviour
{
    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent();
    [HideInInspector]
    public MagazineEvent onMagazineEvent = new MagazineEvent();
    
    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect;   // 총기 이펙트 on / off

    [Header("Spawn Points")] 
    [SerializeField]
    private Transform casingSpawnPoint; //탄피 생성 위치
    [SerializeField]
    private Transform bulletSpawnPoint; //총알 생성 위치

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;   // 무기 장착 사운드
    [SerializeField]
    private AudioClip audioClipFire;   // 공격 사운드
    [SerializeField]
    private AudioClip audioClipReload;   // 재장전 사운드


    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSet weaponSet; // 무기 설정

    private float lastAttackTime = 0; // 마지막 발사 시간 체크
    private bool isReload = false;

    //private float spread_coefficient = 1f;
    private float spread_radius = 0.025f;

    private AudioSource audioSource;
    private PlayerAnimateController animator;
    private CasingMemoryPool casingMemoryPool;
    private ImpactMemoryPool impactMemoryPool; // 공격 효과 생성 후 활성/ 비활성 관리
    private Camera mainCamera;                 // 광선 발사

    //외부에서 열람 가능한 Get 프로퍼티
    public WeaponName WeaponName => weaponSet.weaponName;
    public int CurrentMagazine => weaponSet.currentMagazine;
    public int MaxMagazine => weaponSet.maxMagazine;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInParent<PlayerAnimateController>();
        casingMemoryPool = GetComponent<CasingMemoryPool>();
        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        mainCamera = Camera.main;

        // 처음 탄창 수는 최대로 설정
        weaponSet.currentMagazine = weaponSet.maxMagazine;
        // 처음 탄 수는 최대로 설정
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }

    private void OnEnable()
    {
        // 무기 장착 사운드
        PlaySound(audioClipTakeOutWeapon);
        // 총구 이펙트 오브젝트 비활성화
        muzzleFlashEffect.SetActive(false);
        
        // 무기가 활성화될 때 해당 무기의 탄창 정보 갱신
        onMagazineEvent.Invoke(weaponSet.currentMagazine);
        // 무기가 활성화될 때 탄 수 갱신
        onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);
    }

    private void PlaySound(AudioClip clip) // 기존 사운드 정지 후, 사운드 clip 교체후 재생
    {
        audioSource.Stop();         // 
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StartWeaponAction(int type = 0)
    {
        //재장전 중일 때는 무기 액션 ㄴㄴ
        if(isReload) return;
        
        // 왼쪽 마우스 클릭 (공격 시작
        if (type == 0)
        {
            // ㅡ연속 공격
            if( weaponSet.isAutomaticAttack == true)
            {
                StartCoroutine("OnAttackLoop");
            }
            // 단발 공격
            else
            {
                OnAttack();
            }
        }
        
    }

    public void StopWeaponAction(int type = 0)
    {
        if(type == 0)
        {
            StopCoroutine("OnAttackLoop");
        }
    }

    public void StartReload()
    {
        if(isReload || weaponSet.currentMagazine <= 0) return; //현재 재장전 중이면 꺼지셈
        StopWeaponAction(); //무기 액션 도중 재장전 시도하면 무기 액션 종료하고 재장전
        StartCoroutine("OnReload");
    }

    private IEnumerator OnAttackLoop()
    {
        while (true)
        {
            OnAttack();

            yield return null;
        }
    }

    private void OnAttack()
    {
        if (Time.time - lastAttackTime > weaponSet.attackRate)
        {
            if (animator.MoveSpeed > 0.5f)
            {
                return;
            }

            lastAttackTime = Time.time;
            
            // 탄 수 없으면 공격 X
            if (weaponSet.currentAmmo <= 0)
            {
                return;
            }
            // 탄 감소, 탄 수 감소
            weaponSet.currentAmmo--;
            onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);

            animator.Play("Fire", -1, 0); // 무기 애니메이션
            StartCoroutine("OnMuzzleFlashEffect"); // 총구 이펙트
            PlaySound(audioClipFire); // 총기 발사음
            casingMemoryPool.SpawnCasing(casingSpawnPoint.position, transform.right); //탄피 생성

            TwoStepRaycast();//광선 발사해 원하는 위치 공격
        }
    }

    public IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true);

        yield return new WaitForSeconds(weaponSet.attackRate * 0.3f);

        muzzleFlashEffect.SetActive(false);
    }

    private IEnumerator OnReload()
    {
        isReload = true;
        
        //재장전 애니메이션 사운드 재생
        animator.OnReload();
        PlaySound(audioClipReload);

        while (true)
        {
            // 사운드 재생중이 아니고, 현재 애니메이션이 Movement이면
            // 재장전 애니메이션(, 사운드) 재생이 종료되었다는 뜻
            if (audioSource.isPlaying == false && animator.CurrentAnimationIs("Movement"))
            {
                isReload = false;
                
                // 현재 탄창 수를 1감소하고 바뀐 탄창 정보를 Text UI에 업데이트
                weaponSet.currentMagazine--;
                onMagazineEvent.Invoke(weaponSet.currentMagazine);
                
                //현재 탄 수를 최대로 설정하고, 바뀐 탄 수 정보를 Text UI에 업데이트
                weaponSet.currentAmmo = weaponSet.maxAmmo;
                onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);
                yield break;
            }

            yield return null;
        }
    }

    private void TwoStepRaycast()
    {
        Ray ray;
        RaycastHit  hit;
        Vector3 targetPoint = Vector3.zero;

        // 화면의 중앙 좌표
        ray = Spread_Ray();
        
        
        // 공격 사거리 안에 부딪히는 오브젝트 -> targetpoint는 ㅂ광선에 부딪힌 위치
        if (Physics.Raycast(ray,out hit, weaponSet.attackDistance))
        {
            targetPoint = hit.point;
        }
        // 공격 사거리 안에 부딪히는 오브젝트 X  -> targetpoint는 최대 사거리 위치
        else
        {
            targetPoint = ray.origin + ray.direction*weaponSet.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction * weaponSet.attackDistance, Color.red);

        // 첫번쨰 Raycast 연산으로 얻어진 targetpont를 목표 지점으로 설정, 총구 시작점으로 해 Raycast 연산
        Vector3 attakDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, attakDirection, out hit, weaponSet.attackDistance))
        {
            impactMemoryPool.SpawnImpact(hit);
        }
        Debug.DrawRay(bulletSpawnPoint.position, attakDirection*weaponSet.attackDistance, Color.blue);

    }

    private Ray Spread_Ray()
    {
        //랜덤 생성
        float random_Radius = Random.Range(0, spread_radius);
        float random_Angle = Random.Range(0, 359);
        float random_AngleRad = random_Angle * Mathf.Deg2Rad;
        
        //좌표로 만들기
        Vector3 random_Point = new Vector3(random_Radius * Mathf.Cos(random_AngleRad),random_Radius * Mathf.Sin(random_AngleRad),0f);
        Vector3 random_Point_Normal = random_Point / Camera.main.aspect;
        Vector3 randomViewportPoint = new Vector3(0.5f, 0.5f, 0f) + random_Point_Normal;
        
        Ray ray = mainCamera.ViewportPointToRay(randomViewportPoint);
        
        return ray;
    } 
}
