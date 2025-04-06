using System.Collections;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private TransformMovement movement;
    private float projectileDistance = 30;

    public void Setup(Vector3 position)
    {
        movement = GetComponent<TransformMovement>();
        StartCoroutine("OnMove", position);
    }

    private IEnumerator OnMove(Vector3 targetPosition)
    {
        Vector3 start = transform.position;
        movement.MoveTo((targetPosition - transform.position).normalized);

        while (true)
        {
            if (Vector3.Distance(transform.position, start) >= projectileDistance)
            {
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }
        
    }

    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        Debug.Log("크악 맞앗다니");
        Destroy(gameObject);
    }
}
}