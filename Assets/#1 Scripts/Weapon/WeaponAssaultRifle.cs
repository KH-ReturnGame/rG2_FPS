using System.Collections;
using Unity.Mathematics.Geometry;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;


public class WeaponAssaultRifle : WeaponBase
{
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

    [Header("Aim UI")]
    [SerializeField]
    private Image imageAim;

    private float spread_radius = 0.008f;
    public float spread_Aimmod1_radius = 0.008f;
    public float spread_Aimmod2_radius = 0.001f;
    public GameObject pointPrefab;
    public Canvas canvas;
    public float aimSize = 0.75f;
    private bool isAttak = false;
    private bool isModeChange = false;
    private float defaultModeFOV = 60;
    private float aimModeFOV = 30;
    public float recoil_X = 10;
    public float recoilReturnTime = 0.1f;
    private float recoilOffset = 0f;
    private float offset = 0;
    private float targetOffset = 0f;
    private bool recoilbool = false;
    public RotateToMouse rtm; 
    private float recoilVelocity = 0f;    // SmoothDamp�� ���� ����
    public GameObject weapons;
    Vector3 baseCamEuler;
    public LayerMask rayLayerMask;
    

    private CasingMemoryPool casingMemoryPool;
    private ImpactMemoryPool impactMemoryPool; // ���� ȿ�� ���� �� Ȱ��/ ��Ȱ�� ����
    private Camera mainCamera;                 // ���� �߻�

    private void Awake()
    {
        base.Setup();

        casingMemoryPool = GetComponent<CasingMemoryPool>();
        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        mainCamera = Camera.main;

        // ó�� źâ ���� �ִ�� ����
        weaponSet.currentMagazine = weaponSet.maxMagazine;
        // ó�� ź ���� �ִ�� ����
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }

    private void Start()
    {
        // �ʱ� ���� �̹��� ũ�� ����
        AdjustAimImageSize();
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

    

    public override void StartWeaponAction(int type = 0)
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

    public override void StopWeaponAction(int type = 0)
    {
        if(type == 0)
        {
            isAttak = false;
            StopCoroutine("OnAttackLoop");
        }
    }

    public override void StartReload()
    {
        if(isReload || weaponSet.currentMagazine <= 0 || animator.AimModeIs) return; //���� ������ ���̸� ������
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
            //�ݵ� ����
            //Debug.Log(recoilOffset+"/"+targetOffset+"/"+offset);
            recoilOffset += recoil_X;
            targetOffset += recoil_X;
        }
    }

    private void LateUpdate()
    {
        //rtm.targetOffset = targetOffset;
        if ((!isAttak || weaponSet.currentAmmo <= 0))
        {
            if (!recoilbool)
            {
                // �ǵ��ư� �ݵ� ��ġ�� ��ü�� 3/4 �������� ����
                targetOffset -= recoilOffset / 4f;
                // ������ recoilOffset �ʱ�ȭ
                recoilOffset = 0f;
                recoilbool = true;
            }
            
            // Smoothly move current offset toward target
            offset = Mathf.SmoothDamp(offset, targetOffset, ref recoilVelocity, recoilReturnTime);

            // Apply recoil to camera and weapon rotation
            Vector3 e = baseCamEuler;
            e.x -= offset;
            mainCamera.transform.localEulerAngles = e;
            weapons.transform.localEulerAngles = e;
        }
        
        if ((isAttak && weaponSet.currentAmmo > 0))
        {
            if (recoilbool)
            {
                recoilbool = false;
            }
            
            // Smoothly move current offset toward target
            offset = Mathf.SmoothDamp(offset, targetOffset, ref recoilVelocity, recoilReturnTime/2);

            // Apply recoil to camera and weapon rotation
            Vector3 e = baseCamEuler;
            e.x -= offset;
            mainCamera.transform.localEulerAngles = e;
            weapons.transform.localEulerAngles = e;
        } 
        rtm.targetOffset = targetOffset;
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
        ray = Spread_Ray();
        
        
        // ���� ��Ÿ� �ȿ� �ε����� ������Ʈ -> targetpoint�� �������� �ε��� ��ġ
        if (Physics.Raycast(ray,out hit, weaponSet.attackDistance, rayLayerMask))
        {
            targetPoint = hit.point;
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                switch (hit.transform.tag)
                {
                    case "Enemy_Body":
                        hit.transform.GetComponent<Enemy>().DecreaseHp(weaponSet.AttackDamage);
                        break;
                    case "Enemy_Limbs":
                        hit.collider.gameObject.GetComponentInParent<Enemy>().DecreaseHp(weaponSet.AttackDamage*0.5f);
                        break;
                    case "Enemy_Head":
                        hit.collider.gameObject.GetComponentInParent<Enemy>().DecreaseHp(weaponSet.AttackDamage*2);
                        break;
                }
            }
        }
        // ���� ��Ÿ� �ȿ� �ε����� ������Ʈ X  -> targetpoint�� �ִ� ��Ÿ� ��ġ
        else
        {
            targetPoint = ray.origin + ray.direction*weaponSet.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction * weaponSet.attackDistance, Color.red);

        // ù���� Raycast �������� ����� targetpont�� ��ǥ �������� ����, �ѱ� ���������� �� Raycast ����
        Vector3 attakDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, attakDirection, out hit, weaponSet.attackDistance,rayLayerMask))
        {
            impactMemoryPool.SpawnImpact(hit, attakDirection);
            MakeHole(ray,hit);
        }
        Debug.DrawRay(bulletSpawnPoint.position, attakDirection*weaponSet.attackDistance, Color.blue);
    }

    private Ray Spread_Ray()
    {
        //���� ����
        float random_Radius = Random.Range(0, spread_radius);
        float random_Angle = Random.Range(0, 359);
        float random_AngleRad = random_Angle * Mathf.Deg2Rad;
        
        //��ǥ�� �����
        Vector3 random_Point = new Vector3(random_Radius * Mathf.Cos(random_AngleRad),random_Radius * Mathf.Sin(random_AngleRad),0f);
        float aspect = mainCamera.aspect; // ����/���� ����
        Vector3 random_Point_Adjusted = new Vector3(
            random_Point.x, // x�� �״��
            random_Point.y * aspect, // y�� ȭ������� ���� ���� ����
            0f
        );
        Vector3 randomViewportPoint = new Vector3(0.5f, 0.5f, 0f) + random_Point_Adjusted;
        
        if (pointPrefab != null && canvas != null)
        {
            // ����Ʈ ��ǥ�� ȭ�� ��ǥ�� ��ȯ
            Vector2 screenPoint = new Vector2(
                randomViewportPoint.x * Screen.width,
                randomViewportPoint.y * Screen.height
            );
            //Debug.Log($"Screen Point: {screenPoint}");

            // ȭ�� ��ǥ�� ĵ���� ���� ��ǥ�� ��ȯ
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasPoint;
            bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out canvasPoint
            );

            if (converted)
            {
                //Debug.Log($"Canvas Point: {canvasPoint}");

                // UI �� ����
                GameObject point = Instantiate(pointPrefab, canvas.transform);
                RectTransform pointRect = point.GetComponent<RectTransform>();
                
                // ��Ŀ�� ĵ���� �߾����� �������� �ʰ� ���� ��ġ ����
                pointRect.anchorMin = new Vector2(0.5f, 0.5f);
                pointRect.anchorMax = new Vector2(0.5f, 0.5f);
                pointRect.pivot = new Vector2(0.5f, 0.5f);
                pointRect.anchoredPosition = canvasPoint;

                Destroy(point, 1f); // 5�� �� ����
            }
            else
            {
                Debug.LogWarning("Failed to convert screen point to canvas point.");
            }
        }
        else
        {
            Debug.LogWarning("pointPrefab or canvas is not assigned.");
        }
        
        Ray ray = mainCamera.ViewportPointToRay(randomViewportPoint);
        
        return ray;
    } 
    
    // ���� �̹��� ũ�⸦ �����ϴ� �޼���
    private void AdjustAimImageSize()
    {
        if (imageAim == null || canvas == null) return;

        // �ִ� ������(spread_radius)�� �ش��ϴ� ����Ʈ ��ǥ ���� (x�� ����)
        Vector3 maxRadiusViewportPoint = new Vector3(0.5f + spread_radius * mainCamera.aspect, 0.5f, 0f);
        Vector3 centerViewportPoint = new Vector3(0.5f, 0.5f, 0f);

        // ����Ʈ ��ǥ�� ȭ�� ��ǥ�� ��ȯ
        Vector2 maxRadiusScreenPoint = new Vector2(maxRadiusViewportPoint.x * Screen.width, maxRadiusViewportPoint.y * Screen.height);
        Vector2 centerScreenPoint = new Vector2(centerViewportPoint.x * Screen.width, centerViewportPoint.y * Screen.height);

        // ȭ�� ��ǥ�� ĵ���� ���� ��ǥ�� ��ȯ
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 maxRadiusCanvasPoint, centerCanvasPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            maxRadiusScreenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out maxRadiusCanvasPoint
        );
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            centerScreenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out centerCanvasPoint
        );

        // ĵ���� �����ϸ� ����
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null && scaler.referenceResolution != Vector2.zero)
        {
            float scaleFactor = scaler.referenceResolution.y / Screen.height;
            maxRadiusCanvasPoint *= scaleFactor;
            centerCanvasPoint *= scaleFactor;
        }

        // ĵ���� ���� ������ ��� (x�� ����)
        float canvasRadius = Mathf.Abs(maxRadiusCanvasPoint.x - centerCanvasPoint.x);
        //Debug.Log($"Canvas Radius: {canvasRadius}");

        // ���� �̹��� ũ�� ���� (���� ���� = ������ * 2)
        RectTransform aimRect = imageAim.GetComponent<RectTransform>();
        aimRect.sizeDelta = new Vector2(canvasRadius * aimSize, canvasRadius * aimSize);
    }

    // ȭ�� ũ�� ���� �� ȣ�� (�ɼ�)
    private void OnRectTransformDimensionsChange()
    {
        AdjustAimImageSize();
    }

    public IEnumerator OnModeChange()
    {
        float current = 0;
        float percent = 0;
        float time = 0.1f;
        
        spread_radius = animator.AimModeIs?spread_Aimmod1_radius : spread_Aimmod2_radius;
        AdjustAimImageSize();

        animator.AimModeIs = !animator.AimModeIs;
        //imageAim.enabled = !imageAim.enabled;

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
