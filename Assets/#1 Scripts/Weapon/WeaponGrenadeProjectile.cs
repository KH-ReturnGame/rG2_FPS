using UnityEngine;

public class WeaponGrenadeProjectile : MonoBehaviour
{
    [Header("폭발 설정")]
    [SerializeField]
    private GameObject explosionPrefab;

    [SerializeField] private float explosionRadius = 10.0f; // 폭발 반경
    [SerializeField] private float explosionForce = 500.0f; // 폭발 힘
    [SerializeField] private float throwForece = 1000.0f; // 투척 힘

    private int explosionDamage;
    private new Rigidbody rigidbody;

    public void Setup(int damage, Vector3 rotation)
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(rotation * throwForece); // 주어진 방향으로 수류탄 투척
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
            
            // 폭발 범위 내 물리 오브젝트에 폭발 힘 적용
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