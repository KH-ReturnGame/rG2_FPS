using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState activeState;
    public PatrolState patrolState;
    // property for the patrol state

    public void Initialize()
    {
        // setup defualt
        patrolState = new PatrolState();
        ChangeState(patrolState);
    }

    void Start()
    {

    }

    void Update()
    {
        if (activeState != null)
        {
            activeState.Perform();
        }
    }

    public void ChangeState(BaseState newState)
    {
        // check activeState != null
        if(activeState != null)
        {
            //run cleanup on actvstate
            activeState.Exit();
        }
        // change to a new satet
        activeState = newState;

        //failsafe
        if (activeState != null)
        {
            activeState.stateMachine = this;
            // new state
            activeState.enemy = GetComponent<Enemy>();
            activeState.Enter();
;        }
    }
}


