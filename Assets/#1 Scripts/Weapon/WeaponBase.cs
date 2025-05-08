using UnityEngine;

public enum WeaponType { Main=0, Sub, Melee, Throw }


[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }
[System.Serializable]
public class MagazineEvent : UnityEngine.Events.UnityEvent<int> { }

public abstract class WeaponBase : MonoBehaviour
{
    [Header("WeaponBase")]
    [SerializeField]
    protected WeaponType waeponType; // 무기 종류
    [SerializeField] 
    protected WeaponSet weaponSet;
    [SerializeField]
    protected GameObject bulletHolePrefab;
    [SerializeField]
    protected GameObject bulletHoleContainer;
    

    protected float lastAttackTime = 0; // 마지막 발사 시간
    protected bool isReload = false;
    protected bool isAttack = false;
    protected AudioSource audioSource;
    protected PlayerAnimateController animator;

    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent();
    [HideInInspector]
    public MagazineEvent onMagazineEvent = new MagazineEvent();

    public PlayerAnimateController Animator => animator;
    public WeaponName WeaponName => weaponSet.weaponName;
    public int CurrentMagazine => weaponSet.currentMagazine;
    public int MaxMagazine => weaponSet.maxMagazine;

    public abstract void StartWeaponAction(int type = 0);
    public abstract void StopWeaponAction(int type = 0);
    public abstract void StartReload();


    protected void PlaySound(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
    protected void Setup()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<PlayerAnimateController>();
    }

    protected void MakeHole(Ray ray, RaycastHit hit)
    {
        float positionMultiplier = 0.1f;
        float spawnX = hit.point.x - ray.direction.x * positionMultiplier;
        float spawnY = hit.point.y - ray.direction.y * positionMultiplier;
        float spawnZ = hit.point.z - ray.direction.z * positionMultiplier;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, spawnZ);
        
        GameObject spawnedObj = Instantiate(bulletHolePrefab, hit.point, Quaternion.identity);
        Quaternion targetRotation = Quaternion.LookRotation(ray.direction);

        spawnedObj.transform.rotation = targetRotation;
        spawnedObj.transform.SetParent(bulletHoleContainer.transform);
        Destroy(spawnedObj,10);
    }
}
