using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScansFactory
{

    public class SF_ParentElevator : MonoBehaviour
    {

        private void OnCollisionEnter(Collision collision)
        {

            collision.transform.SetParent(transform);
        }

        private void OnCollisionExit(Collision collision)
        {
            collision.transform.SetParent(null);
        }


    }
}
