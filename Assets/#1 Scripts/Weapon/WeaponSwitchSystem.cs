using UnityEngine;

public class WeaponSwitchSystem : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private WeaponHUD playerHUD;

    [SerializeField]
    private WeaponBase[] weapons; // 소지 중 무기 4종류

    private WeaponBase currentWeapon;
    private WeaponBase previousaWeapon;

    private int prev;
    private int curr =0;

    private void Awake()
    {
        playerHUD.SetupAllWeapons(weapons);

        for (int i= 0; i < weapons.Length; ++i)
        {
            if(weapons[i].gameObject != null)
            {
                weapons[i].gameObject.SetActive(false);
            }    
        }

        SwitchingWeapon(WeaponType.Main);
    }
    private void Update()
    {
        if (Time.timeScale == 0f || !WeaponBase.isWeaponInputEnabled)
            return;
        
        UpdateSwitch();
    }

    private void UpdateSwitch()
    {
        if (!Input.anyKeyDown) return;

        // 1~4 숫자키 누르면 무기 교체
        int inputIndex = 0;
        if(int.TryParse(Input.inputString,out inputIndex)&& (inputIndex > 0 && inputIndex < 5))
        {
            if(curr == 0) // 현재 무기가 AssaultRifle일 때
            {
                // WeaponBase를 WeaponAssaultRifle로 형변환
                WeaponAssaultRifle assaultRifle = currentWeapon as WeaponAssaultRifle;
                if(assaultRifle.RifleMode)
                {
                    // OnModeChange 함수 실행
                    StartCoroutine(assaultRifle.OnModeChange());
                }
            }
            SwitchingWeapon((WeaponType)(inputIndex - 1));
        }
    }

    private void SwitchingWeapon(WeaponType weaponType)
    {
        // 교체 가능 무기 없음녀 종료
        if(weapons[(int)weaponType] == null)
        {
            return;
        }

        if(currentWeapon != null)
        {
            previousaWeapon = currentWeapon;
            prev = curr;
        }

        //무기 교체
        currentWeapon = weapons[(int)weaponType];
        curr = (int)weaponType;

        // 현재 무기로 교체 시 종료
        if (currentWeapon == previousaWeapon)
        {
            return;
        }

        // 무기 사용하는  PlayerController, PlayerHUD에 현재 무기 정보 전달
        player.SwitchingWeapon(currentWeapon);
        playerHUD.SwitchingWeapon(currentWeapon);

        if(previousaWeapon != null)
        {
            previousaWeapon.gameObject.SetActive(false);
        }
        currentWeapon.gameObject.SetActive(true);

    }
}
