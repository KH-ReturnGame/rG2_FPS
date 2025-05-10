using System;
using System.Collections;
using UnityEngine;

public class WeaponKnife : WeaponBase
{
    [SerializeField] private WeaponKnifeCollider weaponKnifeCollider;

    private void OnEnable()
    {
        isAttack = false;
        
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
        
        if(isAttack == true) return;
        
        //연속 공격
        if (weaponSet.isAutomaticAttack == true)
        {
            StartCoroutine("OnAttackLoop", type);
        }
        // 단일 공격
        else
        {
            StartCoroutine("OnAttack", type);
        }
    }
    public override void StopWeaponAction(int type = 0)
    {
        isAttack = false;
        StopCoroutine("OnAttackLoop");
    }

    public override void StartReload()
    {
    }

    private IEnumerator OnAttackLoop(int type)
    {
        while (true)
        {
            yield return StartCoroutine("OnAttack", type);
        }
    }

    private IEnumerator OnAttack(int type)
    {
        isAttack =  true;
        
        //공격 모션 선택 (0,1)
        animator.SetFloat("attackType",type);
        // 공격 애니매이션
        animator.Play("Fire",-1,0);
        
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
    ///애니매이션 이벤트 함수
    /*\\
    public void StartWeaponKnifeCollider()
    {
        WeaponKnifeCollider.StartCollider(weaponSet.damage)
    }
    */
    
}
