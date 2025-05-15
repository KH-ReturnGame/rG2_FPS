using System;
using UnityEngine;

public abstract class Task
{
    protected bool isDone = false;
    protected bool isRunning = false;
    
    public bool IsDone => isDone;
    public bool IsRunning => isRunning;
    
    public virtual void StartTask()
    {
        isRunning = true;
        isDone = false;
        Debug.Log($"Starting task: {GetType().Name}");
    }

    public virtual void InterruptTask()
    {
        isRunning = false;
        Debug.Log($"Interrupting task: {GetType().Name}");
    }
    
    public abstract bool CheckCondition();

    public virtual void EndTask()
    {
        isRunning = false;
        isDone = true;
        if (TutorialManager.Instance != null && TutorialManager.Instance.instructionText != null)
        {
            TutorialManager.Instance.instructionText.text = "";
        }
        Debug.Log($"Completed task: {GetType().Name}");
    }
}

public class MovePlayerTask : Task
{
    private Transform playerTransform;
    private Transform targetTransform;
    private float completionDistance = 1.5f;
    
    public MovePlayerTask(Transform player, Transform target, float distance = 1.5f)
    {
        playerTransform = player;
        targetTransform = target;
        completionDistance = distance;
    }
    
    public override void StartTask()
    {
        base.StartTask();
        
        // Check if transforms are valid
        if (playerTransform == null || targetTransform == null)
        {
            Debug.LogError("MovePlayerTask: Player or target transform is null!");
            EndTask(); // Auto-complete if invalid
            return;
        }
        
        // Activate UI helper or indicator if needed
        // e.g., UIManager.instance.ShowDirectionIndicator(targetTransform.position);
    }

    public override bool CheckCondition()
    {
        if (!isRunning || isDone)
        {
            return isDone;
        }
        
        // Safety check
        if (playerTransform == null || targetTransform == null)
        {
            Debug.LogError("MovePlayerTask: Player or target transform became null during execution!");
            EndTask();
            return true;
        }
        
        float currentDistance = Vector3.Distance(playerTransform.position, targetTransform.position);
        
        // Log distance occasionally for debugging
        if (Time.frameCount % 30 == 0) // Log every 30 frames
        {
            Debug.Log($"Distance to target: {currentDistance}, Need: {completionDistance}");
        }
        
        if (currentDistance <= completionDistance)
        {
            Debug.Log($"MovePlayerTask: Player reached target! Distance: {currentDistance}");
            EndTask();
            return true;
        }
        return false;
    }

    public override void EndTask()
    {
        base.EndTask();
        // Hide UI helper if needed
        // e.g., UIManager.instance.HideDirectionIndicator();
    }
    
    public override void InterruptTask()
    {
        base.InterruptTask();
        // Hide UI helper if needed
        // e.g., UIManager.instance.HideDirectionIndicator();
    }
}

public class WaitForSecondsTask : Task
{
    private float waitTime;
    private float elapsedTime = 0f;
    
    public WaitForSecondsTask(float seconds)
    {
        waitTime = seconds;
    }
    
    public override void StartTask()
    {
        base.StartTask();
        elapsedTime = 0f;
    }
    
    public override bool CheckCondition()
    {
        if (!isRunning || isDone)
        {
            return isDone;
        }
        
        elapsedTime += Time.deltaTime;
        
        // Only log every half second to avoid spamming console
        if (Mathf.FloorToInt(elapsedTime * 2) > Mathf.FloorToInt((elapsedTime - Time.deltaTime) * 2))
        {
            Debug.Log($"WaitTask: {elapsedTime:F1}s / {waitTime:F1}s");
        }
        
        if (elapsedTime >= waitTime)
        {
            Debug.Log($"WaitForSecondsTask: Wait time completed ({waitTime}s)");
            EndTask();
            return true;
        }
        return false;
    }
}

public class InteractionTask : Task
{
    private string requiredInteractionTag;
    private bool interactionDetected = false;
    
    public InteractionTask(string interactionTag)
    {
        requiredInteractionTag = interactionTag;
    }
    
    public override void StartTask()
    {
        base.StartTask();
        interactionDetected = false;
        // Subscribe to interaction events
        // e.g., InteractionSystem.instance.OnInteract += HandleInteraction;
    }
    
    public override bool CheckCondition()
    {
        if (!isRunning || isDone)
        {
            return isDone;
        }
        
        if (interactionDetected)
        {
            Debug.Log($"InteractionTask: Interaction with {requiredInteractionTag} detected");
            EndTask();
            return true;
        }
        return false;
    }
    
    public override void EndTask()
    {
        base.EndTask();
        // Unsubscribe from interaction events
        // e.g., InteractionSystem.instance.OnInteract -= HandleInteraction;
    }
    
    public override void InterruptTask()
    {
        base.InterruptTask();
        // Unsubscribe from interaction events
        // e.g., InteractionSystem.instance.OnInteract -= HandleInteraction;
    }
    
    // Method to be called when interaction happens
    public void HandleInteraction(string interactionTag)
    {
        if (isRunning && requiredInteractionTag == interactionTag)
        {
            Debug.Log($"InteractionTask: Detected interaction with {interactionTag}");
            interactionDetected = true;
        }
    }
}

public class ListenToDialogTask : Task
{
    private DialogData[] dialogs;
    private bool dialogsEnqueued = false;
    
    public ListenToDialogTask(DialogData[] dialogsToListen)
    {
        dialogs = dialogsToListen;
    }
    
    public ListenToDialogTask(DialogData singleDialog)
    {
        dialogs = new DialogData[] { singleDialog };
    }
    
    public override void StartTask()
    {
        base.StartTask();
        
        if (DialogManager.Instance == null)
        {
            Debug.LogError("ListenToDialogTask: DialogManager instance not found!");
            EndTask(); // Auto-complete if invalid
            return;
        }
        
        // Subscribe to dialog state changes
        DialogManager.Instance.onDialogStateChanged.AddListener(OnDialogStateChanged);
        
        // Enqueue dialogs
        DialogManager.Instance.EnqueueDialogs(dialogs);
        dialogsEnqueued = true;
        Debug.Log("ListenToDialogTask: Enqueued dialogs");
    }
    
    public override bool CheckCondition()
    {
        if (!isRunning || isDone)
        {
            return isDone;
        }
        
        // Task completes when all dialogs are finished
        // The actual completion is handled via the event callback
        return isDone;
    }
    
    private void OnDialogStateChanged(bool isActive)
    {
        // When dialog finishes and we've previously enqueued dialogs, complete task
        if (!isActive && dialogsEnqueued)
        {
            Debug.Log("ListenToDialogTask: All dialogs completed");
            
            if (DialogManager.Instance != null)
            {
                DialogManager.Instance.onDialogStateChanged.RemoveListener(OnDialogStateChanged);
            }
            
            EndTask();
        }
    }
    
    public override void EndTask()
    {
        base.EndTask();
        // Ensure we're unsubscribed
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.onDialogStateChanged.RemoveListener(OnDialogStateChanged);
        }
    }
    
    public override void InterruptTask()
    {
        base.InterruptTask();
        
        // Clear all dialogs
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.ClearAllDialogs();
            DialogManager.Instance.onDialogStateChanged.RemoveListener(OnDialogStateChanged);
        }
    }
}

public class MovePlayerWithDialogTask : MovePlayerTask
{
    private DialogData[] dialogs;
    private bool dialogsStarted = false;
    
    public MovePlayerWithDialogTask(Transform player, Transform target, DialogData[] dialogsToShow, float distance = 1.5f) 
        : base(player, target, distance)
    {
        dialogs = dialogsToShow;
    }
    
    public override void StartTask()
    {
        base.StartTask();
        
        // Start dialogs if there are any
        if (dialogs != null && dialogs.Length > 0 && DialogManager.Instance != null)
        {
            DialogManager.Instance.EnqueueDialogs(dialogs);
            dialogsStarted = true;
            Debug.Log("MovePlayerWithDialogTask: Started dialogs");
        }
    }
    
    public override void EndTask()
    {
        base.EndTask();
        
        // Optionally clear dialogs when task ends
        // This depends on your design choice - you might want dialogs to continue
        // For now, we'll let dialogs continue playing
    }
    
    public override void InterruptTask()
    {
        base.InterruptTask();
        
        // If this task started dialogs, clear them
        if (dialogsStarted && DialogManager.Instance != null)
        {
            DialogManager.Instance.ClearAllDialogs();
            Debug.Log("MovePlayerWithDialogTask: Cleared dialogs due to interruption");
        }
    }
}

public class WaitForSecondsWithDialogTask : WaitForSecondsTask
{
    private DialogData[] dialogs;
    private bool dialogsStarted = false;
    
    public WaitForSecondsWithDialogTask(float seconds, DialogData[] dialogsToShow) 
        : base(seconds)
    {
        dialogs = dialogsToShow;
    }
    
    public override void StartTask()
    {
        base.StartTask();
        
        // Start dialogs if there are any
        if (dialogs != null && dialogs.Length > 0 && DialogManager.Instance != null)
        {
            DialogManager.Instance.EnqueueDialogs(dialogs);
            dialogsStarted = true;
            Debug.Log("WaitForSecondsWithDialogTask: Started dialogs");
        }
    }
    
    public override void InterruptTask()
    {
        base.InterruptTask();
        
        // If this task started dialogs, clear them
        if (dialogsStarted && DialogManager.Instance != null)
        {
            DialogManager.Instance.ClearAllDialogs();
            Debug.Log("WaitForSecondsWithDialogTask: Cleared dialogs due to interruption");
        }
    }
}