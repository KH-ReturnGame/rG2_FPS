using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScansFactory
{
    public class SF_ElevatorMoving : MonoBehaviour
    {

        public bool canMove;
        public GameObject chain;

        [SerializeField] float speed;
        [SerializeField] int startPoint;
        [SerializeField] Transform[] points;

        int i;
        bool reverse;



        // Start is called before the first frame update
        void Start()
        {
            transform.position = points[startPoint].position;
            i = startPoint;

        }

        // Update is called once per frame
        void Update()
        {
            if (Vector3.Distance(transform.position, points[i].position) < 0.01f)
            {
                canMove = false;

                if (i == points.Length - 1)
                {
                    reverse = true;
                    i--;
                    return;
                }
                else if (i == 0)
                {
                    chain.SetActive(true);
                    reverse = false;
                    i++;
                    return;
                }

                //target point counter

                if (reverse)
                {
                    i--;
                }
                else
                {
                    i++;
                }
            }


            //move
            if (canMove)
            {
                chain.SetActive(false);
                transform.position = Vector3.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
            }
        }
    }
}
