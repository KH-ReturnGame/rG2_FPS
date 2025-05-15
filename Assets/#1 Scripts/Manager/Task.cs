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
        TutorialManager.Instance.instructionText.text = "";
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
        // Activate UI helper or indicator if needed
        // e.g., UIManager.instance.ShowDirectionIndicator(targetTransform.position);
    }

    public override bool CheckCondition()
    {
        if (!isRunning || isDone) return isDone;
        
        if (Vector3.Distance(playerTransform.position, targetTransform.position) <= completionDistance)
        {
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
        if (!isRunning || isDone) return isDone;
        
        elapsedTime += Time.deltaTime;
        Debug.Log(elapsedTime);
        if (elapsedTime >= waitTime)
        {
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
        if (!isRunning || isDone) return isDone;
        
        if (interactionDetected)
        {
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
            interactionDetected = true;
        }
    }
}