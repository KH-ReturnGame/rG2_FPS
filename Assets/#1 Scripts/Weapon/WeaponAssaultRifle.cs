using System.Collections;
using Unity.Mathematics.Geometry;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;


public class WeaponAssaultRifle : WeaponBase
{
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

    [Header("Aim UI")]
    [SerializeField]
    private Image imageAim;

    private float spread_radius = 0.008f;
    public float spread_Aimmod1_radius = 0.008f;
    public float spread_Aimmod2_radius = 0.001f;
    public GameObject pointPrefab;
    public Canvas canvas;
    public float aimSize = 0.75f;
    private bool isAttak = false;
    private bool isModeChange = false;
    private float defaultModeFOV = 60;
    private float aimModeFOV = 30;
    public float recoil_X = 10;
    public float recoilReturnTime = 0.1f;
    private float recoilOffset = 0f;
    private float offset = 0;
    private float targetOffset = 0f;
    private bool recoilbool = false;
    public RotateToMouse rtm; 
    private float recoilVelocity = 0f;    // SmoothDamp용 내부 변수
    public GameObject weapons;
    Vector3 baseCamEuler;
    public LayerMask rayLayerMask;
    

    private CasingMemoryPool casingMemoryPool;
    private ImpactMemoryPool impactMemoryPool; // 공격 효과 생성 후 활성/ 비활성 관리
    private Camera mainCamera;                 // 광선 발사

    private void Awake()
    {
        base.Setup();

        casingMemoryPool = GetComponent<CasingMemoryPool>();
        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        mainCamera = Camera.main;

        // 처음 탄창 수는 최대로 설정
        weaponSet.currentMagazine = weaponSet.maxMagazine;
        // 처음 탄 수는 최대로 설정
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }

    private void Start()
    {
        // 초기 에임 이미지 크기 설정
        AdjustAimImageSize();
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

        ResetVariables();
    }

    

    public override void StartWeaponAction(int type = 0)
    {
        //재장전 중일 때는 무기 액션 ㄴㄴ
        if(isReload) return;

        //모드 전환중 액션 ㄴ
        if (isModeChange == true) return; 
        
        // 왼쪽 마우스 클릭 (공격 시작
        if (type == 0)
        {
            // ㅡ연속 공격
            if( weaponSet.isAutomaticAttack == true)
            {
                isAttak = true;
                StartCoroutine("OnAttackLoop");
            }
            // 단발 공격
            else
            {
                OnAttack();
            }
        }
        //마우스 오른쪽 클릭 (모드 전환
        else
        {
            //공격 중일 때 모드 전화 ㄴ
            if (isAttak == true) return;

            StartCoroutine("OnModeChange");
        }
        
    }

    public override void StopWeaponAction(int type = 0)
    {
        if(type == 0)
        {
            isAttak = false;
            StopCoroutine("OnAttackLoop");
        }
    }

    public override void StartReload()
    {
        if(isReload || weaponSet.currentMagazine <= 0 || animator.AimModeIs) return; //현재 재장전 중이면 꺼지셈
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

            // 무기 애니메이션 재생 ( 모드에 따라 AimFire or Fire 재생)
            // animator.Play("Fire", -1, 0); // 무기 애니메이션
            string animation = animator.AimModeIs == true ? "AimFire" : "Fire";
            animator.Play(animation, -1, 0);
            
            if ( animator.AimModeIs == false) StartCoroutine("OnMuzzleFlashEffect");// 총구 이펙트
            PlaySound(audioClipFire); // 총기 발사음
            casingMemoryPool.SpawnCasing(casingSpawnPoint.position, transform.right); //탄피 생성

            TwoStepRaycast();//광선 발사해 원하는 위치 공격
            //반동 구현
            //Debug.Log(recoilOffset+"/"+targetOffset+"/"+offset);
            recoilOffset += recoil_X;
            targetOffset += recoil_X;
        }
    }

    private void LateUpdate()
    {
        //rtm.targetOffset = targetOffset;
        if ((!isAttak || weaponSet.currentAmmo <= 0))
        {
            if (!recoilbool)
            {
                // 되돌아갈 반동 위치를 전체의 3/4 지점으로 설정
                targetOffset -= recoilOffset / 4f;
                // 누적된 recoilOffset 초기화
                recoilOffset = 0f;
                recoilbool = true;
            }
            
            // Smoothly move current offset toward target
            offset = Mathf.SmoothDamp(offset, targetOffset, ref recoilVelocity, recoilReturnTime);

            // Apply recoil to camera and weapon rotation
            Vector3 e = baseCamEuler;
            e.x -= offset;
            mainCamera.transform.localEulerAngles = e;
            weapons.transform.localEulerAngles = e;
        }
        
        if ((isAttak && weaponSet.currentAmmo > 0))
        {
            if (recoilbool)
            {
                recoilbool = false;
            }
            
            // Smoothly move current offset toward target
            offset = Mathf.SmoothDamp(offset, targetOffset, ref recoilVelocity, recoilReturnTime/2);

            // Apply recoil to camera and weapon rotation
            Vector3 e = baseCamEuler;
            e.x -= offset;
            mainCamera.transform.localEulerAngles = e;
            weapons.transform.localEulerAngles = e;
        } 
        rtm.targetOffset = targetOffset;
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
        if (Physics.Raycast(ray,out hit, weaponSet.attackDistance, rayLayerMask))
        {
            targetPoint = hit.point;
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                switch (hit.transform.tag)
                {
                    case "Enemy_Body":
                        hit.transform.GetComponent<Enemy>().DecreaseHp(weaponSet.AttackDamage);
                        break;
                    case "Enemy_Limbs":
                        hit.collider.gameObject.GetComponentInParent<Enemy>().DecreaseHp(weaponSet.AttackDamage*0.5f);
                        break;
                    case "Enemy_Head":
                        hit.collider.gameObject.GetComponentInParent<Enemy>().DecreaseHp(weaponSet.AttackDamage*2);
                        break;
                }
            }
        }
        // 공격 사거리 안에 부딪히는 오브젝트 X  -> targetpoint는 최대 사거리 위치
        else
        {
            targetPoint = ray.origin + ray.direction*weaponSet.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction * weaponSet.attackDistance, Color.red);

        // 첫번쨰 Raycast 연산으로 얻어진 targetpont를 목표 지점으로 설정, 총구 시작점으로 해 Raycast 연산
        Vector3 attakDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, attakDirection, out hit, weaponSet.attackDistance,rayLayerMask))
        {
            impactMemoryPool.SpawnImpact(hit, attakDirection);
            MakeHole(ray,hit);
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
        float aspect = mainCamera.aspect; // 가로/세로 비율
        Vector3 random_Point_Adjusted = new Vector3(
            random_Point.x, // x는 그대로
            random_Point.y * aspect, // y를 화면비율로 나눠 원형 유지
            0f
        );
        Vector3 randomViewportPoint = new Vector3(0.5f, 0.5f, 0f) + random_Point_Adjusted;
        
        if (pointPrefab != null && canvas != null)
        {
            // 뷰포트 좌표를 화면 좌표로 변환
            Vector2 screenPoint = new Vector2(
                randomViewportPoint.x * Screen.width,
                randomViewportPoint.y * Screen.height
            );
            //Debug.Log($"Screen Point: {screenPoint}");

            // 화면 좌표를 캔버스 로컬 좌표로 변환
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasPoint;
            bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out canvasPoint
            );

            if (converted)
            {
                //Debug.Log($"Canvas Point: {canvasPoint}");

                // UI 점 생성
                GameObject point = Instantiate(pointPrefab, canvas.transform);
                RectTransform pointRect = point.GetComponent<RectTransform>();
                
                // 앵커를 캔버스 중앙으로 설정하지 않고 직접 위치 지정
                pointRect.anchorMin = new Vector2(0.5f, 0.5f);
                pointRect.anchorMax = new Vector2(0.5f, 0.5f);
                pointRect.pivot = new Vector2(0.5f, 0.5f);
                pointRect.anchoredPosition = canvasPoint;

                Destroy(point, 1f); // 5초 후 삭제
            }
            else
            {
                Debug.LogWarning("Failed to convert screen point to canvas point.");
            }
        }
        else
        {
            Debug.LogWarning("pointPrefab or canvas is not assigned.");
        }
        
        Ray ray = mainCamera.ViewportPointToRay(randomViewportPoint);
        
        return ray;
    } 
    
    // 에임 이미지 크기를 조절하는 메서드
    private void AdjustAimImageSize()
    {
        if (imageAim == null || canvas == null) return;

        // 최대 반지름(spread_radius)에 해당하는 뷰포트 좌표 생성 (x축 기준)
        Vector3 maxRadiusViewportPoint = new Vector3(0.5f + spread_radius * mainCamera.aspect, 0.5f, 0f);
        Vector3 centerViewportPoint = new Vector3(0.5f, 0.5f, 0f);

        // 뷰포트 좌표를 화면 좌표로 변환
        Vector2 maxRadiusScreenPoint = new Vector2(maxRadiusViewportPoint.x * Screen.width, maxRadiusViewportPoint.y * Screen.height);
        Vector2 centerScreenPoint = new Vector2(centerViewportPoint.x * Screen.width, centerViewportPoint.y * Screen.height);

        // 화면 좌표를 캔버스 로컬 좌표로 변환
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 maxRadiusCanvasPoint, centerCanvasPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            maxRadiusScreenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out maxRadiusCanvasPoint
        );
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            centerScreenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out centerCanvasPoint
        );

        // 캔버스 스케일링 보정
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null && scaler.referenceResolution != Vector2.zero)
        {
            float scaleFactor = scaler.referenceResolution.y / Screen.height;
            maxRadiusCanvasPoint *= scaleFactor;
            centerCanvasPoint *= scaleFactor;
        }

        // 캔버스 상의 반지름 계산 (x축 기준)
        float canvasRadius = Mathf.Abs(maxRadiusCanvasPoint.x - centerCanvasPoint.x);
        //Debug.Log($"Canvas Radius: {canvasRadius}");

        // 에임 이미지 크기 조절 (원의 지름 = 반지름 * 2)
        RectTransform aimRect = imageAim.GetComponent<RectTransform>();
        aimRect.sizeDelta = new Vector2(canvasRadius * aimSize, canvasRadius * aimSize);
    }

    // 화면 크기 변경 시 호출 (옵션)
    private void OnRectTransformDimensionsChange()
    {
        AdjustAimImageSize();
    }

    public IEnumerator OnModeChange()
    {
        float current = 0;
        float percent = 0;
        float time = 0.1f;
        
        spread_radius = animator.AimModeIs?spread_Aimmod1_radius : spread_Aimmod2_radius;
        AdjustAimImageSize();

        animator.AimModeIs = !animator.AimModeIs;
        //imageAim.enabled = !imageAim.enabled;

        float start = mainCamera.fieldOfView;
        float end = animator.AimModeIs == true ? aimModeFOV : defaultModeFOV;

        isModeChange = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            //mode 에 따라 시야각 변경
            mainCamera.fieldOfView = Mathf.Lerp(start, end, percent);

            yield return null;
        }
        
        isModeChange = false;
    }

    private void ResetVariables()
    {
        isReload = false;
        isAttak = false;
        isModeChange = false;
    }



}
