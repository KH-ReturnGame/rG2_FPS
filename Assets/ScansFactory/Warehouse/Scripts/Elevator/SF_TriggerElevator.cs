using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScansFactory
{

    public class SF_TriggerElevator : MonoBehaviour
    {

        SF_ElevatorMoving elevator;


        // Start is called before the first frame update
        void Start()
        {
            elevator = GetComponent<SF_ElevatorMoving>();
        }


        private void OnTriggerEnter(Collider other)
        {

            elevator.canMove = true;
        }


    }
}
