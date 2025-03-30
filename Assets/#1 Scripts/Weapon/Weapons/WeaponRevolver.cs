using System.Collections;
using UnityEngine;

public class WeaponRevolver : WeaponBase
{
    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect;   // 총기 이펙트 on / off
    
    [Header("Spawn Points")] 
    [SerializeField]
    private Transform bulletSpawnPoint; //총알 생성 위치
    
    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipFire;   // 공격 사운드
    [SerializeField]
    private AudioClip audioClipReload;   // 재장전 사운드
    
    private ImpactMemoryPool impactMemoryPool; // 공격 효과 생성 후 활성/ 비활성 관리
    private Camera mainCamera;                 // 광선 발사
    
    private void OnEnable()
    {
        // 총구 이펙트 오브젝트 비활성화
        muzzleFlashEffect.SetActive(false);
        
        // 무기가 활성화될 때 해당 무기의 탄창 정보 갱신
        onMagazineEvent.Invoke(weaponSet.currentMagazine);
        // 무기가 활성화될 때 탄 수 갱신
        onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);

        ResetVariables();
    }
    
    private void Awake()
    {
        base.Setup();
        
        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        mainCamera = Camera.main;

        // 처음 탄창 수는 최대로 설정
        weaponSet.currentMagazine = weaponSet.maxMagazine;
        // 처음 탄 수는 최대로 설정
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }
    
    public override void StartWeaponAction(int type = 0)
    {
        if (type == 0 && isReload == false)
        {
            OnAttack();
        }
    }

    public override void StopWeaponAction(int type = 0)
    {
        //
    }

    public override void StartReload()
    {
        if(isReload || weaponSet.currentMagazine <= 0) return; //현재 재장전 중이면 꺼지셈
        StopWeaponAction(); //무기 액션 도중 재장전 시도하면 무기 액션 종료하고 재장전
        StartCoroutine("OnReload");
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
        ray = mainCamera.ViewportPointToRay(Vector3.one * 0.5f);
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

    public void ResetVariables()
    {
        isReload = false;
    }
}
