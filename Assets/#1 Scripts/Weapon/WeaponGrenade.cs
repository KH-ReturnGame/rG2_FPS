using System.Collections;
using UnityEngine;

public class WeaponGrenade : WeaponBase
{
    [Header("Audio Clips")] [SerializeField]
    private AudioClip audioClipFire; // 공격 사운드
    
    [Header("Grenade")]
    [SerializeField]
    private GameObject grenadePrefab;
    [SerializeField]
    private Transform grenadeSpawnPoint;

    private void OnEnable()
    {
        // 무기가 활성화 될 때 해당 무기의 탄창 정보 갱심
        onMagazineEvent.Invoke(weaponSet.currentMagazine);
        // 무기가 활성화 될 때 해당 무기의 탄수 정보 갱심
        onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);
    }
    
    private void Awake()
    {
        base.Setup();
        
        //처음 탄창수 최대
        weaponSet.currentMagazine = weaponSet.maxMagazine;
        //처음 탄 수 최대
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }

    public override void StartWeaponAction(int type = 0)
    {
        if (!WeaponBase.isWeaponInputEnabled) return;
        
        if (type == 0 && isAttack == false && weaponSet.currentAmmo > 0)
        {
            StartCoroutine("OnAttack");
        }
    }

    public override void StopWeaponAction(int type = 0)
    {
    }

    public override void StartReload()
    {
    }
    
    private IEnumerator OnAttack()
    {
        isAttack =  true;
        
        animator.Play("Fire",-1,0);
        PlaySound(audioClipFire);
        
        yield return new WaitForEndOfFrame();

        while (true)
        {
            if (animator.CurrentAnimationIs("Movement"))
            {
                isAttack =  false;
                
                yield break;    
            }
            yield return null;
        }
    }
    
    public void SpawnGrenadeProjectile()
    {
        GameObject granadeClone = Instantiate(grenadePrefab, grenadeSpawnPoint.position, Random.rotation); //random.ROtation
        granadeClone.GetComponent<WeaponGrenadeProjectile>().Setup(0, transform.parent.forward); //weaponSet.damage

        weaponSet.currentAmmo--;
        onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);
    }
    
}
