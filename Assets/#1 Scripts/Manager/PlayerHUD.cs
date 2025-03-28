using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUD : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private WeaponAssaultRifle weapon;    // 현재 정보가 주어지는 무기

    [Header("Weapon Base")]
    [SerializeField]
    private TextMeshProUGUI textWeaponName;   // 무기 이름
    [SerializeField]
    private Image imageWeapon;                // 무기 아이콘
    [SerializeField]
    private Sprite[] spriteWeaponIcons;       // 무기 아이콘이 사용되는 sprite 배열

    [Header("Ammo")]
    [SerializeField]
    private TextMeshProUGUI textAmmo;         // 현재 장전된 탄 수를 표시할 Text

    private void Awake()
    {
        SetupWeapon();
        
        weapon.onAmmoEvent.AddListener(UpdateAmmoHUD);
    }

    private void SetupWeapon()
    {
        textWeaponName.text = weapon.WeaponName.ToString();
        imageWeapon.sprite = spriteWeaponIcons[(int)weapon.WeaponName];
    }
    
    private void UpdateAmmoHUD(int currentAmmo, int maxAmmo)
    {
        textAmmo.text = $"<size=40>{currentAmmo}/</size>{maxAmmo}";
    }
}