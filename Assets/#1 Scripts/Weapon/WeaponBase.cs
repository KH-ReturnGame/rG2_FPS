using UnityEngine;

public enum WeaponType { Main=0, Sub, Melee, Throw }


[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }
[System.Serializable]
public class MagazineEvent : UnityEngine.Events.UnityEvent<int> { }

public class WeaponBase : MonoBehaviour
{
    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent();
    [HideInInspector]
    public MagazineEvent onMagazineEvent = new MagazineEvent();

    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSet weaponSet; // ���� ����

    private float lastAttackTime = 0; // ������ �߻� �ð� üũ
    private bool isReload = false;
    private bool isAttak = false;
        private AudioSource audioSource;
    private PlayerAnimateController animator;















    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
