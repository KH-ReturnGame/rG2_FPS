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
    private GameObject muzzleFlashEffect;   // �ѱ� ����Ʈ on / off

    [Header("Spawn Points")] 
    [SerializeField]
    private Transform casingSpawnPoint; //ź�� ���� ��ġ
    
    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;   // ���� ���� ����
    [SerializeField]
    private AudioClip audioClipFire;   // ���� ����


    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSet weaponSet; // ���� ����

    private float lastAttackTime = 0; // ������ �߻� �ð� üũ

    private AudioSource audioSource;
    private PlayerAnimateController animator;
    private CasingMemoryPool casingMemoryPool;

    //�ܺο��� ���� ������ Get ������Ƽ
    public WeaponName WeaponName => weaponSet.weaponName;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInParent<PlayerAnimateController>();
        casingMemoryPool = GetComponent<CasingMemoryPool>();
        
        // ó�� ź ���� �ִ�� ����
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }

    private void OnEnable()
    {
        // ���� ���� ����
        PlaySound(audioClipTakeOutWeapon);
        // �ѱ� ����Ʈ ������Ʈ ��Ȱ��ȭ
        muzzleFlashEffect.SetActive(false);
        
        //���Ⱑ Ȱ��ȭ�� �� ź �� ����
        onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);
    }

    private void PlaySound(AudioClip clip) // ���� ���� ���� ��, ���� clip ��ü�� ���
    {
        audioSource.Stop();         // 
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StartWeaponAction(int type = 0)
    {
        // ���� ���콺 Ŭ�� (���� ����
        if (type == 0)
        {
            // �ѿ��� ����
            if( weaponSet.isAutomaticAttack == true)
            {
                StartCoroutine("OnAttackLoop");
            }
            // �ܹ� ����
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
            
            // ź �� ������ ���� X
            if (weaponSet.currentAmmo <= 0)
            {
                return;
            }
            // ź ����, ź �� ����
            weaponSet.currentAmmo--;
            onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);

            animator.Play("Fire", -1, 0); // ���� �ִϸ��̼�
            StartCoroutine("OnMuzzleFlashEffect"); // �ѱ� ����Ʈ
            PlaySound(audioClipFire); // �ѱ� �߻���
            casingMemoryPool.SpawnCasing(casingSpawnPoint.position, transform.right); //ź�� ����
        }
    }

    public IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true);

        yield return new WaitForSeconds(weaponSet.attackRate * 0.3f);

        muzzleFlashEffect.SetActive(false);
    }
}
