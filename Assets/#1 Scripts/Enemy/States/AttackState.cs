using UnityEngine;

public class AttackState : BaseState
{
    private float moveTimer;
    private float losePlayerTimer;
    private float shotTimer;
    

    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void Perform()
    {
         if(enemy.CanSeePlayer())
         {
            losePlayerTimer = 0;
            moveTimer += Time.deltaTime;
            shotTimer += Time.deltaTime;
            enemy.transform.LookAt(enemy.Player.transform);
            // if shot > firerate
            if(shotTimer > enemy.fireRate)
            {
                Shoot();
            }
            //
            if(moveTimer > Random.Range(3,7))
            {
                enemy.Agent.SetDestination(enemy.transform.position + (Random.insideUnitSphere * 5));
                moveTimer = 0;
            }
         }
         else
         {
            losePlayerTimer += Time.deltaTime;
            if(losePlayerTimer > 8)
            {
                //change to the search state.
                stateMachine.ChangeState(new PatrolState());
            }
         }
    }
    public void Shoot()
    {
        Debug.Log("Shoot");
        shotTimer = 0;

        Transform gunBarrel = enemy.gunBarrel;
        int damage = enemy.damage;

        Vector3 shootDirection = (enemy.Player.transform.position - gunBarrel.transform.position).normalized;

        Ray ray = new Ray(gunBarrel.transform.position, shootDirection); 
        RaycastHit hit;

        LayerMask playerMask = LayerMask.GetMask("Player");

        if (Physics.Raycast(ray, out hit, 500, playerMask))
        {
              if (hit.collider.gameObject == enemy.Player) {
                   Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.blue);
                   hit.collider.gameObject.GetComponent<Player>().TakeDamage(damage);

              }
              else
              {
                
              }
        }

                    // 광선을 발사하여 장애물이 있는지 확인
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
