using UnityEngine;

public class ParticleAutoDestroyerByTime : MonoBehaviour
{
    private ParticleSystem particle;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        // 파티클 재생중 아님녀 삭제
        if (particle.isPlaying == false)
        {
            Destroy(gameObject);
        }
    }
}
