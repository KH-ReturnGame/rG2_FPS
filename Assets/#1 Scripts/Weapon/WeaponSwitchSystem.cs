using UnityEngine;

public class WeaponSwitchSystem : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private WeaponHUD playerHUD;

    [SerializeField]
    private WeaponBase[] weapons; // ���� �� ���� 4����

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

        // 1~4 ����Ű ������ ���� ��ü
        int inputIndex = 0;
        if(int.TryParse(Input.inputString,out inputIndex)&& (inputIndex > 0 && inputIndex < 5))
        {
            if(curr == 0) // ���� ���Ⱑ AssaultRifle�� ��
            {
                // WeaponBase�� WeaponAssaultRifle�� ����ȯ
                WeaponAssaultRifle assaultRifle = currentWeapon as WeaponAssaultRifle;
                if(assaultRifle.RifleMode)
                {
                    // OnModeChange �Լ� ����
                    StartCoroutine(assaultRifle.OnModeChange());
                }
            }
            SwitchingWeapon((WeaponType)(inputIndex - 1));
        }
    }

    private void SwitchingWeapon(WeaponType weaponType)
    {
        // ��ü ���� ���� ������ ����
        if(weapons[(int)weaponType] == null)
        {
            return;
        }

        if(currentWeapon != null)
        {
            previousaWeapon = currentWeapon;
            prev = curr;
        }

        //���� ��ü
        currentWeapon = weapons[(int)weaponType];
        curr = (int)weaponType;

        // ���� ����� ��ü �� ����
        if (currentWeapon == previousaWeapon)
        {
            return;
        }

        // ���� ����ϴ�  PlayerController, PlayerHUD�� ���� ���� ���� ����
        player.SwitchingWeapon(currentWeapon);
        playerHUD.SwitchingWeapon(currentWeapon);

        if(previousaWeapon != null)
        {
            previousaWeapon.gameObject.SetActive(false);
        }
        currentWeapon.gameObject.SetActive(true);

    }
}
