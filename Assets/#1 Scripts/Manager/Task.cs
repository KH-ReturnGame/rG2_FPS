using System;
using UnityEngine;

// Enums to define task activation and completion conditions
public enum TaskActivationCondition
{
    Immediate,           // Task starts immediately
    CheckPoint,          // Task starts when a specific checkpoint is reached
    PreviousTaskComplete, // Task starts when previous task is completed
    Custom               // Custom condition (requires implementation)
}

public enum TaskCompletionCondition
{
    Checkpoint,          // Task completes when a specific checkpoint is reached
    PlayerAction,        // Task completes when player performs specific action
    Timer,               // Task completes after a specified time
    Custom               // Custom condition (requires implementation)
}

public abstract class Task
{
    protected bool isDone = false;
    protected bool isRunning = false;
    protected bool isPersistent = false; // If true, task won't be interrupted by checkpoints
    
    // Task identification and description
    public string TaskID { get; protected set; }
    public string Description { get; protected set; }
    
    // Activation and completion conditions
    public TaskActivationCondition ActivationCondition { get; protected set; } = TaskActivationCondition.Immediate;
    public TaskCompletionCondition CompletionCondition { get; protected set; } = TaskCompletionCondition.PlayerAction;
    
    public bool IsDone => isDone;
    public bool IsRunning => isRunning;
    public bool IsPersistent => isPersistent;
    
    // Activation/completion parameters
    public int RequiredCheckpoint { get; protected set; } = -1;
    
    // Events
    public event Action<Task> OnTaskStarted;
    public event Action<Task> OnTaskCompleted;
    public event Action<Task> OnTaskInterrupted;
    
    public Task(string taskId, string description)
    {
        TaskID = taskId;
        Description = description;
    }
    
    public virtual void StartTask()
    {
        isRunning = true;
        isDone = false;
        Debug.Log($"Starting task: {TaskID} - {Description}");
        OnTaskStarted?.Invoke(this);
    }

    public virtual void InterruptTask()
    {
        // Only allow interruption if task is not persistent
        if (!isPersistent)
        {
            isRunning = false;
            Debug.Log($"Interrupting task: {TaskID}");
            OnTaskInterrupted?.Invoke(this);
        }
        else
        {
            Debug.Log($"Task {TaskID} is persistent and cannot be interrupted");
        }
    }
    
    public abstract bool CheckCondition();

    public virtual void EndTask()
    {
        isRunning = false;
        isDone = true;
        Debug.Log($"Completed task: {TaskID}");
        OnTaskCompleted?.Invoke(this);
    }
    
    // Method to set task as persistent (won't be interrupted by checkpoints)
    public Task SetPersistent(bool persistent)
    {
        isPersistent = persistent;
        return this;
    }
    
    // Method to set required checkpoint for activation/completion
    public Task SetRequiredCheckpoint(int checkpoint)
    {
        RequiredCheckpoint = checkpoint;
        return this;
    }
    
    // Method to set activation condition
    public Task SetActivationCondition(TaskActivationCondition condition)
    {
        ActivationCondition = condition;
        return this;
    }
    
    // Method to set completion condition
    public Task SetCompletionCondition(TaskCompletionCondition condition)
    {
        CompletionCondition = condition;
        return this;
    }
}

public class MovePlayerTask : Task
{
    private Transform playerTransform;
    private Transform targetTransform;
    private float completionDistance = 1.5f;
    
    public MovePlayerTask(string taskId, string description, Transform player, Transform target, float distance = 1.5f)
        : base(taskId, description)
    {
        playerTransform = player;
        targetTransform = target;
        completionDistance = distance;
        CompletionCondition = TaskCompletionCondition.PlayerAction;
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
        
        // Check completion condition
        if (CompletionCondition == TaskCompletionCondition.PlayerAction)
        {
            if (Vector3.Distance(playerTransform.position, targetTransform.position) <= completionDistance)
            {
                EndTask();
                return true;
            }
        }
        
        return false;
    }

    public override void EndTask()
    {
        base.EndTask();
        // Hide UI helper if needed
        // e.g., UIManager.instance.HideDirectionIndicator();
    }
}

public class WaitForSecondsTask : Task
{
    private float waitTime;
    private float elapsedTime = 0f;
    
    public WaitForSecondsTask(string taskId, string description, float seconds)
        : base(taskId, description)
    {
        waitTime = seconds;
        CompletionCondition = TaskCompletionCondition.Timer;
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
        if (elapsedTime >= waitTime)
        {
            EndTask();
            return true;
        }
        return false;
    }
}

public class CheckpointTask : Task
{
    public CheckpointTask(string taskId, string description, int requiredCheckpoint)
        : base(taskId, description)
    {
        RequiredCheckpoint = requiredCheckpoint;
        CompletionCondition = TaskCompletionCondition.Checkpoint;
    }
    
    public override bool CheckCondition()
    {
        // This task is completed externally by the TutorialManager when the checkpoint is reached
        return isDone;
    }
    
    // Method to mark the task as complete when checkpoint is reached
    public void CheckpointReached(int checkpointNumber)
    {
        if (isRunning && checkpointNumber == RequiredCheckpoint)
        {
            EndTask();
        }
    }
}

public class InputTask : Task
{
    private KeyCode requiredKey;
    private bool keyPressed = false;
    
    public InputTask(string taskId, string description, KeyCode key)
        : base(taskId, description)
    {
        requiredKey = key;
        CompletionCondition = TaskCompletionCondition.PlayerAction;
    }
    
    public override void StartTask()
    {
        base.StartTask();
        keyPressed = false;
    }
    
    public override bool CheckCondition()
    {
        if (!isRunning || isDone) return isDone;
        
        if (Input.GetKeyDown(requiredKey))
        {
            keyPressed = true;
            EndTask();
            return true;
        }
        return false;
    }
}

public class MovementInputTask : Task
{
    private KeyCode[] requiredKeys;
    private bool[] keysPressed;
    private bool requireAllKeys;
    
    public MovementInputTask(string taskId, string description, KeyCode[] keys, bool requireAll = false)
        : base(taskId, description)
    {
        requiredKeys = keys;
        keysPressed = new bool[keys.Length];
        requireAllKeys = requireAll;
        CompletionCondition = TaskCompletionCondition.PlayerAction;
    }
    
    public override void StartTask()
    {
        base.StartTask();
        for (int i = 0; i < keysPressed.Length; i++)
        {
            keysPressed[i] = false;
        }
    }
    
    public override bool CheckCondition()
    {
        if (!isRunning || isDone) return isDone;
        
        // Check for key presses
        bool completed = requireAllKeys;
        
        for (int i = 0; i < requiredKeys.Length; i++)
        {
            if (Input.GetKeyDown(requiredKeys[i]))
            {
                keysPressed[i] = true;
            }
            
            if (requireAllKeys)
            {
                completed &= keysPressed[i];
            }
            else if (keysPressed[i])
            {
                completed = true;
                break;
            }
        }
        
        if (completed)
        {
            EndTask();
            return true;
        }
        
        return false;
    }
}