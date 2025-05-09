using UnityEngine;
using System.Collections;

public class WeaponRevolver : WeaponBase
{

    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect; // �ѱ� ����Ʈ

    [Header("Spawn Points")]
    [SerializeField]
    private Transform bulletSpawnPoint;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipFire;
    [SerializeField]
    private AudioClip audioClipReload;

    private ImpactMemoryPool impactMemoryPool;
    private Camera mainCamera;

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
    private float recoilVelocity = 0f;    // SmoothDamp�� ���� ����
    Vector3 baseCamEuler;

    private void OnEnable()
    {
        // �ѱ� ����Ʈ ������Ʈ ��Ȱ��ȭ
        muzzleFlashEffect.SetActive(false);

        // ���Ⱑ Ȱ��ȭ �� �� ���ش� ������ źâ ������ �����Ѵ�
        onMagazineEvent.Invoke(weaponSet.currentMagazine);
        // ���Ⱑ Ȱ��ȭ �� �� �ش� ������ ź�� ������ �����Ѵ�.
        onAmmoEvent.Invoke(weaponSet.currentAmmo, weaponSet.maxAmmo);

        ResetVariables();
    }

    private void Awake()
    {
        base.Setup();

        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        mainCamera = Camera.main;

        //ó�� źâ ���� �ִ�� ����
        weaponSet.currentMagazine = weaponSet.maxMagazine;
        //ó�� ź ���� �ִ�� ����
        weaponSet.currentAmmo = weaponSet.maxAmmo;
    }
    public override void StartWeaponAction(int type = 0)
    {
        if (type == 0 && isAttack == false && isReload == false)
        {
            OnAttack();
        }
    }

    public override void StopWeaponAction(int type = 0)
    {
        isAttack = false;
    }

    public override void StartReload()
    {
        if (isReload == true || weaponSet.currentMagazine <= 0) return;

        // ���� �׼� �� r -> ��� �� ������
        StopWeaponAction();
        StartCoroutine("OnReload");
    }

    public void OnAttack()
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
            animator.Play("Fire", -1, 0);

            StartCoroutine("OnMuzzleFlashEffect");// �ѱ� ����Ʈ

            TwoStepRaycast();//���� �߻��� ���ϴ� ��ġ ����
            PlaySound(audioClipFire);
            //�ݵ� ����
            recoilOffset += recoil_X;
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
        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        // ȭ���� �߾� ��ǥ
        ray = Spread_Ray();


        // ���� ��Ÿ� �ȿ� �ε����� ������Ʈ -> targetpoint�� �������� �ε��� ��ġ
        if (Physics.Raycast(ray, out hit, weaponSet.attackDistance))
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
            targetPoint = ray.origin + ray.direction * weaponSet.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction * weaponSet.attackDistance, Color.red);

        // ù���� Raycast �������� ����� targetpont�� ��ǥ �������� ����, �ѱ� ���������� �� Raycast ����
        Vector3 attakDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, attakDirection, out hit, weaponSet.attackDistance))
        {
            impactMemoryPool.SpawnImpact(hit, attakDirection);
            MakeHole(ray,hit);
        }
        Debug.DrawRay(bulletSpawnPoint.position, attakDirection * weaponSet.attackDistance, Color.blue);
    }

    private Ray Spread_Ray()
    {
        //���� ����
        float random_Radius = Random.Range(0, spread_radius);
        float random_Angle = Random.Range(0, 359);
        float random_AngleRad = random_Angle * Mathf.Deg2Rad;

        //��ǥ�� �����
        Vector3 random_Point = new Vector3(random_Radius * Mathf.Cos(random_AngleRad), random_Radius * Mathf.Sin(random_AngleRad), 0f);
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
            Debug.Log($"Screen Point: {screenPoint}");

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
                Debug.Log($"Canvas Point: {canvasPoint}");

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
    private void ResetVariables()
    {
        isReload = false;
        isAttack = false;
    }
}
