using System.Collections;
using UnityEngine;

[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }

public class WeaponAssaultRifle : MonoBehaviour
{
    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent();
    
    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect;   // 총기 이펙트 on / off

    [Header("Spawn Points")] 
    [SerializeField]
    private Transform casingSpawnPoint; //탄피 생성 위치
    
    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;   // 무기 장착 사운드
    [SerializeField]
    private AudioClip audioClipFire;   // 공격 사운드


    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSet weaponSet; // 무기 설정

    private float lastAttackTime = 0; // 마지막 발사 시간 체크

    private AudioSource audioSource;
    private PlayerAnimateController animator;
    private CasingMemoryPool casingMemoryPool;

    //외부에서 열람 가능한 Get 프로퍼티
    public WeaponName WeaponName => weaponSet.weaponName;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInParent<PlayerAnimateController>();
        casingMemoryPool = GetComponent<CasingMemoryPool>();
        
        // 처음 탄 수는 최대로 설정
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }

    private void OnEnable()
    {
        // 무기 장착 사운드
        PlaySound(audioClipTakeOutWeapon);
        // 총구 이펙트 오브젝트 비활성화
        muzzleFlashEffect.SetActive(false);
        
        //무기가 활성화될 때 탄 수 갱신
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
        }
    }

    public IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true);

        yield return new WaitForSeconds(weaponSet.attackRate * 0.3f);

        muzzleFlashEffect.SetActive(false);
    }
}
