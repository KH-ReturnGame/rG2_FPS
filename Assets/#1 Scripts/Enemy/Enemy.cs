using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private StateMachine stateMachine;
    private NavMeshAgent agent;

    public NavMeshAgent Agent { get => agent; }
    // 디버그용
    [SerializeField]
    private string currentState;
    public Path path;
    private GameObject player;
    public float sightDistance = 50f;
    public float fieldOfView = 85f;

    public LayerMask playerMask;

    public LayerMask obstacleMask;
    public LayerMask obstacleMask2;
    
    [Header("HP")]
    [SerializeField]
    private float maxHP = 100f;
    private float currentHP;
    
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    
    [HideInInspector]
    public HPEvent onHPEvent = new HPEvent();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Initialize();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Awake()
    {
        currentHP = maxHP;
    }

    public bool DecreaseHp(float damage)
    {
        float previousHP = currentHP;
        if (currentHP - damage > 0)
        {
            currentHP -= damage;
        }
        else
        {
            Destroy(gameObject);
        }

        onHPEvent.Invoke(previousHP, currentHP);
        
        Debug.Log("에너미 체력감소 ㅠㅠ : "+previousHP+"->"+currentHP);
        
        if (currentHP == 0)
        {
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        CanSeePlayer();
    }

    public bool CanSeePlayer()
    {
        /*
        if(player != null)
        {
            // player close
            if (Vector3.Distance(transform.position,player.transform.position) < sightDistance)
            {
                Vector3 targetDirection = player.transform.position - transform.position;
                float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
                if(angleToPlayer >= -fieldOfView && angleToPlayer <= fieldOfView)
                {
                   Ray ray = new Ray(transform.position, targetDirection);
                   Debug.DrawRay(ray.origin, ray.direction * sightDistance);
                }
            }
        }
        return true;
    }
    */

     if (player != null)
        {
            Vector3 targetDirection = player.transform.position - transform.position;
            float distanceToPlayer = targetDirection.magnitude;

            // 시야 거리 안에 있는지 확인
            if (distanceToPlayer < sightDistance)
            {
                // 시야 각도 안에 있는지 확인
                float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
                if (angleToPlayer >= -fieldOfView && angleToPlayer <= fieldOfView)
                {
                    
                    Ray ray = new Ray(transform.position, targetDirection.normalized); // 적의 눈높이에서 광선 발사
                    RaycastHit hit;

                    // 광선을 발사하여 장애물이 있는지 확인
                    if (Physics.Raycast(ray, out hit, sightDistance, obstacleMask) || Physics.Raycast(ray, out hit, sightDistance, obstacleMask2))
                    {
                        // 광선에 부딪힌 오브젝트가 플레이어가 아니라면 플레이어를 감지할 수 없음
                        if (hit.collider.gameObject != player)
                        {
                            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow); // 장애물에 막힌 광선 (노란색)
                            return false;
                            Debug.Log(hit.collider.gameObject.name);
                        }
                        else
                        {
                            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green); // 플레이어를 감지한 광선 (초록색)
                            return true;
                        }
                    }
                    else
                    {
                        // 장애물이 없다면 플레이어가 시야 내에 있음
                        Debug.DrawRay(ray.origin, ray.direction * sightDistance, Color.green); // 플레이어를 감지한 광선 (초록색)
                        return true;
                    }
                   

                    //  Ray ray = new Ray(transform.position, targetDirection.normalized);
                    // RaycastHit hit;
                    //
                    // // obstacleMask에 있는 오브젝트에 먼저 부딪히는지 확인
                    // if (Physics.Raycast(ray, out hit, sightDistance, obstacleMask) || Physics.Raycast(ray, out hit, sightDistance, obstacleMask2))
                    // {
                    //     Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow); // 장애물에 막힌 광선 (노란색)
                    //     return false;
                    // }
                    // // 장애물에 부딪히지 않았다면 플레이어가 보이는지 확인
                    // else
                    // {
                    //     // 플레이어 레이어만 필터링하여 광선 발사
                    //     if (Physics.Raycast(ray, out hit, sightDistance, playerMask))
                    //     {
                    //         if ((hit.collider.gameObject == player) || (hit.collider.gameObject.name == "PlayerDetection"))
                    //         {
                    //             Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green); // 플레이어를 감지한 광선 (초록색)
                    //             return true;
                    //         }
                    //     }
                    //     Debug.DrawRay(ray.origin, ray.direction * sightDistance, Color.red); // 플레이어가 시야 내에 있지만 감지되지 않음 (빨간색)
                    //     return false;
                    // }
                }
                else
                {
                    Debug.DrawRay(transform.position , targetDirection.normalized * sightDistance, Color.red); // 시야 각도 밖 (빨간색)
                    return false;
                }
            }
            else
            {
                return false; // 시야 거리 밖
            }
        }
        return false; // 플레이어 오브젝트가 없음
    }
}
