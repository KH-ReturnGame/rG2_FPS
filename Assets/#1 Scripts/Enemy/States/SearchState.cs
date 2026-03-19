using UnityEngine;

public class SearchState : BaseState
{
    private float searchTimer;
    private float moveTimer;

    public float searchLimit = 5;

    public override void Enter()
    {
        enemy.Agent.SetDestination(enemy.LastKnownPos);
    }

    public override void Perform()
    {
        if(enemy.CanSeePlayer())
            stateMachine.ChangeState(new AttackState());

        if(enemy.Agent.remainingDistance < enemy.Agent.stoppingDistance)
        {
            searchTimer += Time.deltaTime;
            moveTimer += Time.deltaTime;
            if(moveTimer > Random.Range(3,5))
            {
                enemy.Agent.SetDestination(enemy.transform.position + (Random.insideUnitSphere * 8));
                moveTimer = 0;
            }
            if(searchTimer > searchLimit)
            {
                stateMachine.ChangeState(new PatrolState());
            }
        }
    }

    public override void Exit()
    {

    }
}
