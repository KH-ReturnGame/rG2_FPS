using UnityEngine;

public class PatrolState : BaseState
{
    public int waypointIndex;
    public float waitTimer;
    public override void Enter()
    {

    }

    public override void Perform()
    {
        PatrolCycle();
        if(enemy.CanSeePlayer())
        {
            stateMachine.ChangeState(new AttackState());
        }
    }

    public override void Exit()
    {

    }

    public void PatrolCycle()
    {
         if (enemy.Agent.remainingDistance < 0.2f)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer > Random.Range(1, 3)) 
                {
                     waypointIndex++;
                     if (waypointIndex >= enemy.path.waypoints.Count)
                     {
                         waypointIndex = 0;
                     }
                     // 웨이포인트가 여전히 존재하는지 확인 (혹시라도 리스트가 변경되었을 경우 대비)
                     if (waypointIndex < enemy.path.waypoints.Count)
                     {
                         enemy.Agent.SetDestination(enemy.path.waypoints[waypointIndex].position);
                     }
                     else
                     {
                         Debug.LogWarning("PatrolCycle: 웨이포인트 인덱스가 범위를 벗어났습니다. (예상치 않은 상황)");
                         waypointIndex = 0; // 인덱스 초기화하여 안전하게 유지
                     }
                     waitTimer = 0;
                }
              
            }
    }
}

/* if (waypointIndex < enemy.path.waypoints.Count - 1)
                waypointIndex++;
            else
                waypointIndex = 0;
            enemy.Agent.SetDestination(enemy.path.waypoints[waypointIndex].position);*/
