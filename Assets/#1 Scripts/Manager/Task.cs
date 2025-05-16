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
        Debug.Log($"ListenToDialogTask: Enqueued {dialogs.Length} dialogs");
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
    
    public MovePlayerWithDialogTask(Transform player, Transform target, DialogData singleDialog, float distance = 1.5f)
        : base(player, target, distance)
    {
        dialogs = new DialogData[] { singleDialog };
    }
    
    public override void StartTask()
    {
        base.StartTask();
        
        // Start dialogs if there are any
        if (dialogs != null && dialogs.Length > 0 && DialogManager.Instance != null)
        {
            DialogManager.Instance.EnqueueDialogs(dialogs);
            dialogsStarted = true;
            Debug.Log($"MovePlayerWithDialogTask: Started {dialogs.Length} dialogs");
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
    
    public WaitForSecondsWithDialogTask(float seconds, DialogData singleDialog)
        : base(seconds)
    {
        dialogs = new DialogData[] { singleDialog };
    }
    
    public override void StartTask()
    {
        base.StartTask();
        
        // Start dialogs if there are any
        if (dialogs != null && dialogs.Length > 0 && DialogManager.Instance != null)
        {
            DialogManager.Instance.EnqueueDialogs(dialogs);
            dialogsStarted = true;
            Debug.Log($"WaitForSecondsWithDialogTask: Started {dialogs.Length} dialogs");
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

public class KeyPressTask : Task
{
    private KeyCode requiredKey;
    private float requiredPressTime;
    private float currentPressTime = 0f;
    private bool keyWasPressed = false;
    private string instructionText;
    
    // Constructor for single key
    public KeyPressTask(KeyCode key, float pressTimeRequired, string instruction)
    {
        requiredKey = key;
        requiredPressTime = pressTimeRequired;
        instructionText = instruction;
    }
    
    public override void StartTask()
    {
        base.StartTask();
        currentPressTime = 0f;
        keyWasPressed = false;
        
        // Display instruction
        if (TutorialManager.Instance != null && TutorialManager.Instance.instructionText != null)
        {
            TutorialManager.Instance.instructionText.text = instructionText;
        }
    }
    
    public override bool CheckCondition()
    {
        if (!isRunning || isDone)
        {
            return isDone;
        }
        
        // Check for key press
        if (Input.GetKey(requiredKey))
        {
            keyWasPressed = true;
            currentPressTime += Time.deltaTime;
            
            // Debug log (occasionally)
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"KeyPressTask: Pressing {requiredKey} for {currentPressTime:F1}s / {requiredPressTime:F1}s");
            }
            
            // Check if completed
            if (currentPressTime >= requiredPressTime)
            {
                Debug.Log($"KeyPressTask: Successfully pressed {requiredKey} for {requiredPressTime} seconds!");
                
                EndTask();
                return true;
            }
        }
        else if (keyWasPressed)
        {
            // Key was released before completion - reset timer but don't reset completely
            currentPressTime = Mathf.Max(0, currentPressTime - (Time.deltaTime * 2));
        }
        
        return false;
    }
    
    public override void EndTask()
    {
        base.EndTask();
    }
}

public class MultiKeyPressTask : Task
{
    private KeyCode[] requiredKeys;
    private float requiredPressTime;
    private float[] currentPressTimes;
    private bool[] keysPressed;
    private string instructionText;
    
    // Constructor for multiple keys that need to be pressed sequentially
    public MultiKeyPressTask(KeyCode[] keys, float pressTimeRequired, string instruction)
    {
        requiredKeys = keys;
        requiredPressTime = pressTimeRequired;
        instructionText = instruction;
        
        currentPressTimes = new float[keys.Length];
        keysPressed = new bool[keys.Length];
    }
    
    public override void StartTask()
    {
        base.StartTask();
        
        for (int i = 0; i < currentPressTimes.Length; i++)
        {
            currentPressTimes[i] = 0f;
            keysPressed[i] = false;
        }
        
        // Display instruction
        if (TutorialManager.Instance != null && TutorialManager.Instance.instructionText != null)
        {
            TutorialManager.Instance.instructionText.text = instructionText;
        }
    }
    
    public override bool CheckCondition()
    {
        if (!isRunning || isDone)
        {
            return isDone;
        }
        
        bool allKeysCompleted = true;
        int activeKeyIndex = -1;
        
        // Find the first incomplete key
        for (int i = 0; i < requiredKeys.Length; i++)
        {
            if (currentPressTimes[i] < requiredPressTime)
            {
                allKeysCompleted = false;
                if (activeKeyIndex == -1)
                {
                    activeKeyIndex = i;
                }
                break;
            }
        }
        
        // If all keys completed, end task
        if (allKeysCompleted)
        {
            Debug.Log("MultiKeyPressTask: All keys completed!");
            
            EndTask();
            return true;
        }
        
        // Process active key
        if (activeKeyIndex >= 0)
        {
            KeyCode activeKey = requiredKeys[activeKeyIndex];
            
            // Check for key press
            if (Input.GetKey(activeKey))
            {
                keysPressed[activeKeyIndex] = true;
                currentPressTimes[activeKeyIndex] += Time.deltaTime;
                
                // Debug log (occasionally)
                if (Time.frameCount % 30 == 0)
                {
                    Debug.Log($"MultiKeyPressTask: Pressing {activeKey} for {currentPressTimes[activeKeyIndex]:F1}s / {requiredPressTime:F1}s");
                }
                
                // Check if current key completed
                if (currentPressTimes[activeKeyIndex] >= requiredPressTime)
                {
                    Debug.Log($"MultiKeyPressTask: Successfully pressed {activeKey} for {requiredPressTime} seconds!");
                    
                    
                    // If we're on the last key, end task
                    if (activeKeyIndex == requiredKeys.Length - 1)
                    {
                        EndTask();
                        return true;
                    }
                    
                    // Update instruction text for next key
                    if (TutorialManager.Instance != null && TutorialManager.Instance.instructionText != null)
                    {
                        string nextKeyName = requiredKeys[activeKeyIndex + 1].ToString();
                        TutorialManager.Instance.instructionText.text = $"[{nextKeyName} 키를 눌러 주세요.]";
                    }
                }
            }
            else if (keysPressed[activeKeyIndex])
            {
                // Key was released before completion - reset timer but don't reset completely
                currentPressTimes[activeKeyIndex] = Mathf.Max(0, currentPressTimes[activeKeyIndex] - (Time.deltaTime * 2));
            }
        }
        
        return false;
    }
    
    public override void EndTask()
    {
        base.EndTask();
    }
}