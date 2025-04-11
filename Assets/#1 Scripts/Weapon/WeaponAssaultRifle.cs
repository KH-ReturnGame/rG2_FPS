using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }
[System.Serializable]
public class MagazineEvent : UnityEngine.Events.UnityEvent<int> { }


public class WeaponAssaultRifle : MonoBehaviour
{
    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent();
    [HideInInspector]
    public MagazineEvent onMagazineEvent = new MagazineEvent();
    
    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect;   // �ѱ� ����Ʈ on / off

    [Header("Spawn Points")] 
    [SerializeField]
    private Transform casingSpawnPoint; //ź�� ���� ��ġ
    [SerializeField]
    private Transform bulletSpawnPoint; //�Ѿ� ���� ��ġ

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;   // ���� ���� ����
    [SerializeField]
    private AudioClip audioClipFire;   // ���� ����
    [SerializeField]
    private AudioClip audioClipReload;   // ������ ����


    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSet weaponSet; // ���� ����

    [Header("Aim UI")]
    [SerializeField]
    private Image imageAim;

    private float lastAttackTime = 0; // ������ �߻� �ð� üũ
    private bool isReload = false;
    private bool isAttak = false;
    private bool isModeChange = false;
    private float defaultModeFOV = 60;
    private float aimModeFOV = 30;


    private AudioSource audioSource;
    private PlayerAnimateController animator;
    private CasingMemoryPool casingMemoryPool;
    private ImpactMemoryPool impactMemoryPool; // ���� ȿ�� ���� �� Ȱ��/ ��Ȱ�� ����
    private Camera mainCamera;                 // ���� �߻�

    //�ܺο��� ���� ������ Get ������Ƽ
    public WeaponName WeaponName => weaponSet.weaponName;
    public int CurrentMagazine => weaponSet.currentMagazine;
    public int MaxMagazine => weaponSet.maxMagazine;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInParent<PlayerAnimateController>();
        casingMemoryPool = GetComponent<CasingMemoryPool>();
        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        mainCamera = Camera.main;

        // ó�� źâ ���� �ִ�� ����
        weaponSet.currentMagazine = weaponSet.maxMagazine;
        // ó�� ź ���� �ִ�� ����
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }

    private void OnEnable()
    {
        // ���� ���� ����
        PlaySound(audioClipTakeOutWeapon);
        // �ѱ� ����Ʈ ������Ʈ ��Ȱ��ȭ
        muzzleFlashEffect.SetActive(false);
        
        // ���Ⱑ Ȱ��ȭ�� �� �ش� ������ źâ ���� ����
        onMagazineEvent.Invoke(weaponSet.currentMagazine);
        // ���Ⱑ Ȱ��ȭ�� �� ź �� ����
        onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);

        ResetVariables();
    }

    private void PlaySound(AudioClip clip) // ���� ���� ���� ��, ���� clip ��ü�� ���
    {
        audioSource.Stop();         // 
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StartWeaponAction(int type = 0)
    {
        //������ ���� ���� ���� �׼� ����
        if(isReload) return;

        //��� ��ȯ�� �׼� ��
        if (isModeChange == true) return; 
        
        // ���� ���콺 Ŭ�� (���� ����
        if (type == 0)
        {
            // �ѿ��� ����
            if( weaponSet.isAutomaticAttack == true)
            {
                isAttak = true;
                StartCoroutine("OnAttackLoop");
            }
            // �ܹ� ����
            else
            {
                OnAttack();
            }
        }
        //���콺 ������ Ŭ�� (��� ��ȯ
        else
        {
            //���� ���� �� ��� ��ȭ ��
            if (isAttak == true) return;

            StartCoroutine("OnModeChange");
        }
        
    }

    public void StopWeaponAction(int type = 0)
    {
        if(type == 0)
        {
            isAttak = false;
            StopCoroutine("OnAttackLoop");
        }
    }

    public void StartReload()
    {
        if(isReload || weaponSet.currentMagazine <= 0) return; //���� ������ ���̸� ������
        StopWeaponAction(); //���� �׼� ���� ������ �õ��ϸ� ���� �׼� �����ϰ� ������
        StartCoroutine("OnReload");
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

            // ���� �ִϸ��̼� ��� ( ��忡 ���� AimFire or Fire ���)
            // animator.Play("Fire", -1, 0); // ���� �ִϸ��̼�
            string animation = animator.AimModeIs == true ? "AimFire" : "Fire";
            animator.Play(animation, -1, 0);
            if ( animator.AimModeIs == false) StartCoroutine("OnMuzzleFlashEffect");// �ѱ� ����Ʈ
            PlaySound(audioClipFire); // �ѱ� �߻���
            casingMemoryPool.SpawnCasing(casingSpawnPoint.position, transform.right); //ź�� ����

            TwoStepRaycast();//���� �߻��� ���ϴ� ��ġ ����
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
        
        //������ �ִϸ��̼� ���� ���
        animator.OnReload();
        PlaySound(audioClipReload);

        while (true)
        {
            // ���� ������� �ƴϰ�, ���� �ִϸ��̼��� Movement�̸�
            // ������ �ִϸ��̼�(, ����) ����� ����Ǿ��ٴ� ��
            if (audioSource.isPlaying == false && animator.CurrentAnimationIs("Movement"))
            {
                isReload = false;
                
                // ���� źâ ���� 1�����ϰ� �ٲ� źâ ������ Text UI�� ������Ʈ
                weaponSet.currentMagazine--;
                onMagazineEvent.Invoke(weaponSet.currentMagazine);
                
                //���� ź ���� �ִ�� �����ϰ�, �ٲ� ź �� ������ Text UI�� ������Ʈ
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

        // ȭ���� �߾� ��ǥ
        ray = mainCamera.ViewportPointToRay(Vector3.one * 0.5f);
        // ���� ��Ÿ� �ȿ� �ε����� ������Ʈ -> targetpoint�� �������� �ε��� ��ġ
        if (Physics.Raycast(ray,out hit, weaponSet.attackDistance))
        {
            targetPoint = hit.point;
        }
        // ���� ��Ÿ� �ȿ� �ε����� ������Ʈ X  -> targetpoint�� �ִ� ��Ÿ� ��ġ
        else
        {
            targetPoint = ray.origin + ray.direction*weaponSet.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction * weaponSet.attackDistance, Color.red);

        // ù���� Raycast �������� ����� targetpont�� ��ǥ �������� ����, �ѱ� ���������� �� Raycast ����
        Vector3 attakDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, attakDirection, out hit, weaponSet.attackDistance))
        {
            impactMemoryPool.SpawnImpact(hit);
        }
        Debug.DrawRay(bulletSpawnPoint.position, attakDirection*weaponSet.attackDistance, Color.blue);

    }

    private IEnumerator OnModeChange()
    {
        float current = 0;
        float percent = 0;
        float time = 0.35f;

        animator.AimModeIs = !animator.AimModeIs;
        imageAim.enabled = !imageAim.enabled;

        float start = mainCamera.fieldOfView;
        float end = animator.AimModeIs == true ? aimModeFOV : defaultModeFOV;

        isModeChange = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            //mode �� ���� �þ߰� ����
            mainCamera.fieldOfView = Mathf.Lerp(start, end, percent);

            yield return null;
        }

        isModeChange = false;
    }

    private void ResetVariables()
    {
        isReload = false;
        isAttak = false;
        isModeChange = false;
    }



}
