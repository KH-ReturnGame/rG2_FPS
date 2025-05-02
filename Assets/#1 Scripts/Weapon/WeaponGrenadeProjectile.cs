using UnityEngine;

public class WeaponGrenadeProjectile : MonoBehaviour
{
    [Header("Explosion Barrel")] [SerializeField]
    private GameObject explosionPrefab;

    [SerializeField] private float explosionRadius = 10.0f;
    [SerializeField] private float explosionForce = 500.0f;
    [SerializeField] private float throwForece = 1000.0f;

    private int explosionDamage;
    private new Rigidbody rigidbody;

    public void Setup(int damage, Vector3 rotation)
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(rotation * throwForece);
        explosionDamage = damage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 폭발 이펙트
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        
        //범위 안 오브젝트 collider 정보 받아와 폭발 효과 처리
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Debug.Log("Damage");
            
            Rigidbody rigidbody = hit.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.AddExplosionForce(explosionForce,transform.position,explosionRadius);
            }
        }
        
        // 수류탄 삭제
        Destroy(gameObject);
    }
}