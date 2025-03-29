using System.Collections.Generic;
using NUnit.Framework;
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

    [Header("Magazine")] 
    [SerializeField] 
    private GameObject magazineUIPrefab; //탄창 UI 프리팹
    [SerializeField] 
    private Transform magazineParent; //탄창 UI가 배치되는 Panel
    private List<GameObject> magazineList; //탄창 UI 리스트
    

    private void Awake()
    {
        SetupWeapon();
        SetupMagazine();
        
        weapon.onAmmoEvent.AddListener(UpdateAmmoHUD);
        weapon.onMagazineEvent.AddListener(UpdateMagazinHUD);
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
    
    private void SetupMagazine()
    {
        //weapon에 등록되어 있는 탄창 개수만큼 Image Icon 생성
        //magazineParent 오브젝트의 자식으로 등록 후 모두 비활성화/리스트에 저장
        magazineList = new List<GameObject>();
        for (int i = 0; i < weapon.MaxMagazine; ++i)
        {
            GameObject clone = Instantiate(magazineUIPrefab);
            clone.transform.SetParent(magazineParent);
            clone.SetActive(false);
            
            magazineList.Add(clone);
        }
        
        //weapon에 등록되어 있는 현재 탄창 개수만큼 오브젝트 활성화
        for (int i = 0; i < weapon.CurrentMagazine; ++i)
        {
            magazineList[i].SetActive(true);
        }
    }

    private void UpdateMagazinHUD(int currentMagazine)
    {
        //전부 비활성화하고, currentMagazine 개수만큼 활성화
        for (int i = 0; i < magazineList.Count; ++i)
        {
            magazineList[i].SetActive(false);
        }
        for (int i = 0; i < currentMagazine; ++i)
        {
            magazineList[i].SetActive(true);
        }
    }
}