using UnityEngine;
using System.Collections;

public class WeaponRevolver : WeaponBase
{

    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect; // 총구 이펙트

    [Header("Spawn Points")]
    [SerializeField]
    private Transform bulletSpawnPoint;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipFire;
    [SerializeField]
    private AudioClip audioClipReload;

    private ImpactMemoryPool impactMemoryPool;
    private Camera mainCamera;

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
    private float recoilVelocity = 0f;    // SmoothDamp용 내부 변수
    Vector3 baseCamEuler;

    private void OnEnable()
    {
        // 총구 이펙트 오브젝트 비활성화
        muzzleFlashEffect.SetActive(false);

        // 무기가 활성화 될 때 ㅅ해당 무기의 탄창 정보를 갱신한다
        onMagazineEvent.Invoke(weaponSet.currentMagazine);
        // 무기가 활성화 될 때 해당 무기의 탄수 정보를 갱신한다.
        onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);

        ResetVariables();
    }

    private void Awake()
    {
        base.Setup();

        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        mainCamera = Camera.main;

        //처음 탄창 수는 최대로 설정
        weaponSet.currentMagazine = weaponSet.maxMagazine;
        //처음 탄 수는 최대로 설정
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }
    public override void StartWeaponAction(int type = 0)
    {
        if (type == 0 && isAttack == false && isReload == false)
        {
            OnAttack();
        }
    }

    public override void StopWeaponAction(int type = 0)
    {
        isAttack = false;
    }

    public override void StartReload()
    {
        if (isReload == true || weaponSet.currentMagazine <= 0) return;

        // 무기 액션 중 r -> 취소 후 재장전
        StopWeaponAction();
        StartCoroutine("OnReload");
    }

    public void OnAttack()
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
            animator.Play("Fire", -1, 0);

            StartCoroutine("OnMuzzleFlashEffect");// 총구 이펙트

            TwoStepRaycast();//광선 발사해 원하는 위치 공격
            PlaySound(audioClipFire);
            //반동 구현
            recoilOffset += recoil_X;
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
        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        // 화면의 중앙 좌표
        ray = Spread_Ray();


        // 공격 사거리 안에 부딪히는 오브젝트 -> targetpoint는 ㅂ광선에 부딪힌 위치
        if (Physics.Raycast(ray, out hit, weaponSet.attackDistance))
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
            targetPoint = ray.origin + ray.direction * weaponSet.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction * weaponSet.attackDistance, Color.red);

        // 첫번쨰 Raycast 연산으로 얻어진 targetpont를 목표 지점으로 설정, 총구 시작점으로 해 Raycast 연산
        Vector3 attakDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, attakDirection, out hit, weaponSet.attackDistance))
        {
            impactMemoryPool.SpawnImpact(hit, attakDirection);
            MakeHole(ray,hit);
        }
        Debug.DrawRay(bulletSpawnPoint.position, attakDirection * weaponSet.attackDistance, Color.blue);
    }

    private Ray Spread_Ray()
    {
        //랜덤 생성
        float random_Radius = Random.Range(0, spread_radius);
        float random_Angle = Random.Range(0, 359);
        float random_AngleRad = random_Angle * Mathf.Deg2Rad;

        //좌표로 만들기
        Vector3 random_Point = new Vector3(random_Radius * Mathf.Cos(random_AngleRad), random_Radius * Mathf.Sin(random_AngleRad), 0f);
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
            Debug.Log($"Screen Point: {screenPoint}");

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
                Debug.Log($"Canvas Point: {canvasPoint}");

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
    private void ResetVariables()
    {
        isReload = false;
        isAttack = false;
    }
}
