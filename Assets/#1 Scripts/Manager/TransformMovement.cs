using UnityEngine;

public class TransformMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 0.0f;

    [SerializeField]
    private Vector3 moveDirection = Vector3.zero;


    private void Update()
    {
        Debug.Log("?ASDDSASDAD");
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

  
    public void MoveTo(Vector3 direction)
    {
        moveDirection = direction;
    }
}