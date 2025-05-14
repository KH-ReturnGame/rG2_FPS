using System;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private int checkPointNumber = 0;
    [SerializeField] private bool activateOnce = true;
    [SerializeField] private GameObject visualIndicator;
    
    private bool hasBeenActivated = false;
    
    public int CheckPointNumber => checkPointNumber;
    
    private void Start()
    {
        // Optional: Set up visual indicator
        if (visualIndicator != null)
        {
            visualIndicator.SetActive(false);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!activateOnce || !hasBeenActivated))
        {
            // Record activation
            hasBeenActivated = true;
            
            // Show visual feedback
            if (visualIndicator != null)
            {
                visualIndicator.SetActive(true);
            }
            
            // Invoke checkpoint event
            TutorialManager tutorialManager = TutorialManager.Instance;
            if (tutorialManager != null)
            {
                tutorialManager.cp_event.Invoke(gameObject, checkPointNumber);
            }
            else
            {
                Debug.LogError("CheckPoint: TutorialManager instance not found!");
            }
            
            // Optional - disable collider if one-time use
            if (activateOnce)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }
    
    // Editor-only: Draw checkpoint visual in scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = hasBeenActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // Draw checkpoint number
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"Checkpoint {checkPointNumber}");
#endif
    }
}