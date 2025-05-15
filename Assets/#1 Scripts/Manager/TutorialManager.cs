using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointEvent : UnityEngine.Events.UnityEvent<GameObject, int> { }

[Serializable]
public class DialogSettings
{
    public bool includeDialog = false;
    
    // Changed from single dialog to list of dialogs
    [SerializeField]
    public List<DialogEntry> dialogEntries = new List<DialogEntry>();

    // Default typewriter settings for all dialogs (can be overridden per entry)
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.05f;
}

// New class to represent individual dialog entries
[Serializable]
public class DialogEntry
{
    [TextArea(2, 5)]
    public string speakerName;
    [TextArea(3, 10)]
    public string dialogText;
    public AudioClip dialogVoiceClip;
    public float dialogDuration = 3.0f;
    
    // Per-dialog typewriter settings
    public bool overrideTypewriterSettings = false;
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.05f;
}

public class TutorialManager : MonoBehaviour
{
    // Singleton pattern
    public static TutorialManager Instance { get; private set; }
    
    // Checkpoint event
    public CheckPointEvent cp_event = new CheckPointEvent();

    // Tutorial progress tracking
    public int CurrentState = 0;
    public int CurrentCheckpoint = 0;
    
    // Task management
    private Task currentTask;
    private Queue<Task> taskQueue = new Queue<Task>();
    
    // Player reference
    [SerializeField] private Transform playerTransform;
    
    // Tutorial step definitions
    [Serializable]
    public class TutorialStep
    {
        public string description;
        public int requiredCheckpoint;
        public GameObject targetObject;
        
        // Add KeyPressTask support
        [Tooltip("move, interact, wait, dialog, key-press, multi-key-press")]
        public string taskType; 
        public float taskParameter; // distance for move, seconds for wait, duration for key press
        
        // For key press tasks
        public KeyCode primaryKey = KeyCode.None;
        public KeyCode[] multiKeys = new KeyCode[0];
        
        // Dialog Settings
        public DialogSettings dialogSettings = new DialogSettings();
    }
    
    [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    
    // UI References
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] public TMPro.TextMeshProUGUI instructionText;
    //[SerializeField] public ProgressBar progressBar;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        //DontDestroyOnLoad(gameObject);
        
        // Set up checkpoint listener
        cp_event.AddListener(CheckNextStep);
    }
    
    private void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("TutorialManager: Player not found! Please assign playerTransform or tag a GameObject as 'Player'");
            }
        }
        
        // Start first tutorial step
        StartTutorialStep(CurrentState);
    }

    private void Update()
    {
        // Check current task condition
        if (currentTask != null && currentTask.IsRunning)
        {
            bool taskCompleted = currentTask.CheckCondition();
            if (taskCompleted)
            {
                Debug.Log($"Task completed: {currentTask.GetType().Name}");
                ProcessNextTask();
            }
        }
    }

    private void StartTutorialStep(int stepIndex)
    {
        Debug.Log($"Starting tutorial step {stepIndex}");
        
        if (stepIndex >= tutorialSteps.Count)
        {
            CompleteTutorial();
            return;
        }
        
        TutorialStep step = tutorialSteps[stepIndex];
        
        // Update UI
        if (instructionText != null && tutorialUI != null)
        {
            tutorialUI.SetActive(true);
            instructionText.text = step.description;
        }
        
        // Create dialog data if this step has dialog
        DialogData[] stepDialogs = null;
        if (step.dialogSettings.includeDialog && step.dialogSettings.dialogEntries.Count > 0)
        {
            // Convert DialogEntry list to DialogData array
            stepDialogs = new DialogData[step.dialogSettings.dialogEntries.Count];
            
            for (int i = 0; i < step.dialogSettings.dialogEntries.Count; i++)
            {
                DialogEntry entry = step.dialogSettings.dialogEntries[i];
                
                stepDialogs[i] = new DialogData
                {
                    speakerName = entry.speakerName,
                    dialogText = entry.dialogText,
                    displayDuration = entry.dialogDuration,
                    voiceClip = entry.dialogVoiceClip,
                    
                    // Use per-dialog override settings if specified,
                    // otherwise use the settings from dialogSettings
                    overrideTypewriterSettings = entry.overrideTypewriterSettings,
                    useTypewriterEffect = entry.overrideTypewriterSettings ? 
                                          entry.useTypewriterEffect : 
                                          step.dialogSettings.useTypewriterEffect,
                    typewriterSpeed = entry.overrideTypewriterSettings ? 
                                      entry.typewriterSpeed : 
                                      step.dialogSettings.typewriterSpeed
                };
            }
        }
        
        // Create appropriate task based on step type
        switch (step.taskType.ToLower())
        {
            case "move":
                if (step.targetObject != null)
                {
                    if (step.dialogSettings.includeDialog && stepDialogs != null && stepDialogs.Length > 0)
                    {
                        AddTask(new MovePlayerWithDialogTask(playerTransform, step.targetObject.transform, stepDialogs, step.taskParameter));
                    }
                    else
                    {
                        AddTask(new MovePlayerTask(playerTransform, step.targetObject.transform, step.taskParameter));
                    }
                }
                else
                {
                    Debug.LogError($"TutorialManager: Target object missing for move task in step {stepIndex}");
                }
                break;
                
            case "wait":
                if (step.dialogSettings.includeDialog && stepDialogs != null && stepDialogs.Length > 0)
                {
                    AddTask(new WaitForSecondsWithDialogTask(step.taskParameter, stepDialogs));
                }
                else
                {
                    AddTask(new WaitForSecondsTask(step.taskParameter));
                }
                break;
                
            case "dialog":
                if (step.dialogSettings.includeDialog && stepDialogs != null && stepDialogs.Length > 0)
                {
                    AddTask(new ListenToDialogTask(stepDialogs));
                }
                else
                {
                    Debug.LogError($"TutorialManager: Dialog task type specified but no dialog configured in step {stepIndex}");
                }
                break;
                
            case "key-press":
                if (step.primaryKey != KeyCode.None)
                {
                    // Create a key press task
                    KeyPressTask keyTask = new KeyPressTask(step.primaryKey, step.taskParameter, step.description);
                    AddTask(keyTask);
                }
                else
                {
                    Debug.LogError($"TutorialManager: Key-press task with no key specified in step {stepIndex}");
                }
                break;
                
            case "multi-key-press":
                if (step.multiKeys != null && step.multiKeys.Length > 0)
                {
                    // Create a multi-key press task
                    MultiKeyPressTask multiKeyTask = new MultiKeyPressTask(step.multiKeys, step.taskParameter, step.description);
                    AddTask(multiKeyTask);
                }
                else
                {
                    Debug.LogError($"TutorialManager: Multi-key-press task with no keys specified in step {stepIndex}");
                }
                break;
                
            default:
                Debug.LogError($"TutorialManager: Unknown task type '{step.taskType}' in step {stepIndex}");
                break;
        }
        
        ProcessNextTask();
    }
    
    public void AddTask(Task task)
    {
        taskQueue.Enqueue(task);
        Debug.Log($"Added task: {task.GetType().Name}, Task queue count: {taskQueue.Count}");
    }
    
    private void ProcessNextTask()
    {
        Debug.Log($"Processing next task. Current state: {CurrentState}, Current CP: {CurrentCheckpoint}");
        
        // End current task if it exists
        if (currentTask != null && currentTask.IsRunning)
        {
            currentTask.EndTask();
            Debug.Log("Ended current task");
        }
    
        // Start next task if available
        if (taskQueue.Count > 0)
        {
            currentTask = taskQueue.Dequeue();
            currentTask.StartTask();
            Debug.Log($"Starting next task: {currentTask.GetType().Name}, Remaining tasks: {taskQueue.Count}");
        }
        else
        {
            Debug.Log("No more tasks in queue");
            currentTask = null;
        
            // 마지막 step인지 확인
            if (CurrentState >= tutorialSteps.Count - 1)
            {
                CompleteTutorial();
                return;
            }
        
            // Check if we should advance to next tutorial step
            if (CurrentState + 1 < tutorialSteps.Count && 
                tutorialSteps[CurrentState + 1].requiredCheckpoint <= CurrentCheckpoint)
            {
                Debug.Log($"Advancing to next tutorial step. Current state: {CurrentState} -> {CurrentState + 1}");
                CurrentState++;
                StartTutorialStep(CurrentState);
            }
            else
            {
                Debug.Log($"Not advancing to next tutorial step. Next step requires checkpoint: {tutorialSteps[CurrentState + 1].requiredCheckpoint}, Current CP: {CurrentCheckpoint}");
            }
        }
    }
    
    public void CheckNextStep(GameObject sender, int checkpointNumber)
    {
        Debug.Log($"Checkpoint triggered: {checkpointNumber}, Current CP: {CurrentCheckpoint}");
        
        // Skip if this checkpoint is less than our current progress
        if (checkpointNumber < CurrentCheckpoint)
        {
            Debug.Log($"Ignoring checkpoint {checkpointNumber} as we're already at {CurrentCheckpoint}");
            return;
        }
        
        // Update checkpoint progress
        CurrentCheckpoint = checkpointNumber + 1;
        Debug.Log($"Updated CurrentCheckpoint to {CurrentCheckpoint}");
        
        // Find the appropriate tutorial state for this checkpoint
        int targetState = CurrentState;
        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (tutorialSteps[i].requiredCheckpoint <= CurrentCheckpoint && i > targetState)
            {
                targetState = i-1;
                Debug.Log($"Found suitable state: {i} for checkpoint {CurrentCheckpoint}");
            }
        }
        
        // Only change state if we need to move forward
        if (targetState > CurrentState)
        {
            Debug.Log($"Advancing state: {CurrentState} -> {targetState}");
            
            // Interrupt current task if it exists
            if (currentTask != null && currentTask.IsRunning)
            {
                Debug.Log($"Interrupting current task: {currentTask.GetType().Name}");
                currentTask.InterruptTask();
            }
            
            // Clear task queue
            int queueCount = taskQueue.Count;
            taskQueue.Clear();
            Debug.Log($"Cleared {queueCount} tasks from queue");
            
            // Update state and start new step
            CurrentState = targetState;
            StartTutorialStep(CurrentState);
        }
        else
        {
            Debug.Log($"Not advancing state as targetState ({targetState}) <= CurrentState ({CurrentState})");
        }
    }
    
    private void CompleteTutorial()
    {
        Debug.Log("Tutorial completed!");
        
        // Hide tutorial UI
        if (tutorialUI != null)
        {
            tutorialUI.SetActive(false);
        }

        instructionText.text = "Tutorial completed!";
    }
    
    // Helper method to create a DialogData from a DialogEntry
    public DialogData CreateDialogData(DialogEntry entry, DialogSettings settings = null)
    {
        DialogData dialog = new DialogData
        {
            speakerName = entry.speakerName,
            dialogText = entry.dialogText,
            displayDuration = entry.dialogDuration,
            voiceClip = entry.dialogVoiceClip
        };
        
        // Apply typewriter settings
        if (entry.overrideTypewriterSettings)
        {
            dialog.overrideTypewriterSettings = true;
            dialog.useTypewriterEffect = entry.useTypewriterEffect;
            dialog.typewriterSpeed = entry.typewriterSpeed;
        }
        else if (settings != null)
        {
            dialog.overrideTypewriterSettings = true;
            dialog.useTypewriterEffect = settings.useTypewriterEffect;
            dialog.typewriterSpeed = settings.typewriterSpeed;
        }
        
        return dialog;
    }
    
    // Start a single dialog (helper method)
    public void StartDialog(DialogData dialog)
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.EnqueueDialog(dialog);
        }
        else
        {
            Debug.LogError("TutorialManager: DialogManager instance not found!");
        }
    }
    
    // Start multiple dialogs (helper method)
    public void StartDialogs(DialogData[] dialogs)
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.EnqueueDialogs(dialogs);
        }
        else
        {
            Debug.LogError("TutorialManager: DialogManager instance not found!");
        }
    }
    
    // Start dialogs from DialogEntry list
    public void StartDialogsFromEntries(List<DialogEntry> entries, DialogSettings settings = null)
    {
        if (DialogManager.Instance != null && entries != null && entries.Count > 0)
        {
            DialogData[] dialogs = new DialogData[entries.Count];
            
            for (int i = 0; i < entries.Count; i++)
            {
                dialogs[i] = CreateDialogData(entries[i], settings);
            }
            
            DialogManager.Instance.EnqueueDialogs(dialogs);
        }
        else if (entries == null || entries.Count == 0)
        {
            Debug.LogError("TutorialManager: Cannot start dialogs from empty entries!");
        }
        else
        {
            Debug.LogError("TutorialManager: DialogManager instance not found!");
        }
    }
    
    // Skip current dialog
    public void SkipCurrentDialog()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.SkipCurrentDialog();
        }
    }
    
    // Clear all dialogs
    public void ClearAllDialogs()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.ClearAllDialogs();
        }
    }
}