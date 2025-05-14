using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointEvent : UnityEngine.Events.UnityEvent<GameObject, int> { }

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
    private List<Task> activeTasks = new List<Task>(); // Allow multiple simultaneous tasks
    private Queue<Task> taskQueue = new Queue<Task>();
    
    // Player reference
    [SerializeField] private Transform playerTransform;
    
    // Tutorial step definitions
    [Serializable]
    public class TutorialStep
    {
        public string id;
        public string description;
        public int requiredCheckpoint = -1;
        public GameObject targetObject;
        public string taskType; // "move", "input", "wait", "checkpoint", "movement"
        [Tooltip("Key for Input task, seconds for Wait task, distance for Move task")]
        public float taskParameter;
        [Tooltip("Whether this task should persist until completed, even if checkpoints are triggered")]
        public bool isPersistent = false;
        [Tooltip("Tutorial messages to show when this step starts")]
        public List<DialogueSystem.DialogueEntry> dialogueEntries = new List<DialogueSystem.DialogueEntry>();
        [Tooltip("Whether to show dialogue before or after starting the task")]
        public bool showDialogueBeforeTask = true;
    }
    
    [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    
    // UI References (optional)
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private TMPro.TextMeshProUGUI instructionText;
    
    // Flag to track if we're waiting for dialogue to complete
    private bool isWaitingForDialogue = false;
    private int pendingStepIndex = -1;
    
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
        
        // Listen for dialogue completion events
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.OnDialogueSequenceComplete += OnDialogueComplete;
        }
        
        // Start first tutorial step
        StartTutorialStep(CurrentState);
    }

    private void Update()
    {
        // Check all active tasks' conditions
        for (int i = activeTasks.Count - 1; i >= 0; i--)
        {
            Task task = activeTasks[i];
            if (task.IsRunning && task.CheckCondition())
            {
                // Task completed, remove from active tasks
                activeTasks.RemoveAt(i);
                
                // Process next task if needed
                if (activeTasks.Count == 0 && taskQueue.Count > 0)
                {
                    ProcessNextTask();
                }
                else if (activeTasks.Count == 0)
                {
                    // No active tasks and no queued tasks, advance tutorial if possible
                    CheckAdvanceTutorial();
                }
            }
        }
    }

    private void StartTutorialStep(int stepIndex)
    {
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
        
        // Check if we should show dialogue before starting the task
        if (step.dialogueEntries.Count > 0 && step.showDialogueBeforeTask)
        {
            // Start dialogue and wait for completion
            isWaitingForDialogue = true;
            pendingStepIndex = stepIndex;
            DialogueSystem.Instance.StartDialogueSequence(step.dialogueEntries);
            return;
        }
        
        // Create and start the task
        CreateTaskForStep(step);
    }
    
    private void CreateTaskForStep(TutorialStep step)
    {
        Task newTask = null;
        
        // Create appropriate task based on step type
        switch (step.taskType.ToLower())
        {
            case "move":
                if (step.targetObject != null)
                {
                    newTask = new MovePlayerTask(step.id, step.description, 
                        playerTransform, step.targetObject.transform, step.taskParameter);
                }
                else
                {
                    Debug.LogError($"TutorialManager: Target object missing for move task in step {step.id}");
                }
                break;
                
            case "wait":
                newTask = new WaitForSecondsTask(step.id, step.description, step.taskParameter);
                break;
                
            case "checkpoint":
                newTask = new CheckpointTask(step.id, step.description, (int)step.taskParameter);
                break;
                
            case "input":
                newTask = new InputTask(step.id, step.description, (KeyCode)step.taskParameter);
                break;
                
            case "movement":
                // Create a movement task with WASD keys
                KeyCode[] movementKeys = new KeyCode[] 
                { 
                    KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D 
                };
                newTask = new MovementInputTask(step.id, step.description, movementKeys, false);
                break;
                
            default:
                Debug.LogError($"TutorialManager: Unknown task type '{step.taskType}' in step {step.id}");
                break;
        }
        
        // Setup task properties
        if (newTask != null)
        {
            newTask.SetPersistent(step.isPersistent);
            if (step.requiredCheckpoint >= 0)
            {
                newTask.SetRequiredCheckpoint(step.requiredCheckpoint);
            }
            
            // Add task to queue
            AddTask(newTask);
            
            // Show dialogue after task if needed
            if (step.dialogueEntries.Count > 0 && !step.showDialogueBeforeTask)
            {
                DialogueSystem.Instance.StartDialogueSequence(step.dialogueEntries);
            }
        }
    }
    
    public void AddTask(Task task)
    {
        taskQueue.Enqueue(task);
        
        // Start task immediately if no tasks are running
        if (activeTasks.Count == 0)
        {
            ProcessNextTask();
        }
    }
    
    private void ProcessNextTask()
    {
        if (taskQueue.Count > 0)
        {
            Task nextTask = taskQueue.Dequeue();
            activeTasks.Add(nextTask);
            nextTask.StartTask();
        }
    }
    
    private void CheckAdvanceTutorial()
    {
        // Check if we should advance to next tutorial step
        if (CurrentState < tutorialSteps.Count && 
            (tutorialSteps[CurrentState].requiredCheckpoint == -1 || 
             tutorialSteps[CurrentState].requiredCheckpoint <= CurrentCheckpoint))
        {
            CurrentState++;
            StartTutorialStep(CurrentState);
        }
    }
    
    public void CheckNextStep(GameObject sender, int checkpointNumber)
    {
        Debug.Log($"Checkpoint triggered: {checkpointNumber}, Current CP: {CurrentCheckpoint}");
        
        // Update checkpoint progress
        if (checkpointNumber == CurrentCheckpoint)
        {
            CurrentCheckpoint++;
            
            // Notify any active checkpoint tasks
            foreach (Task task in activeTasks)
            {
                if (task is CheckpointTask checkpointTask)
                {
                    checkpointTask.CheckpointReached(checkpointNumber);
                }
            }
            
            // Interrupt non-persistent tasks
            for (int i = activeTasks.Count - 1; i >= 0; i--)
            {
                Task task = activeTasks[i];
                if (!task.IsPersistent)
                {
                    task.InterruptTask();
                    activeTasks.RemoveAt(i);
                }
            }
            
            // Clear task queue except for persistent tasks
            Queue<Task> persistentTasks = new Queue<Task>();
            while (taskQueue.Count > 0)
            {
                Task queuedTask = taskQueue.Dequeue();
                if (queuedTask.IsPersistent)
                {
                    persistentTasks.Enqueue(queuedTask);
                }
            }
            taskQueue = persistentTasks;
            
            // Check if this checkpoint should trigger next tutorial step
            if (CurrentState < tutorialSteps.Count && 
                tutorialSteps[CurrentState].requiredCheckpoint <= CurrentCheckpoint)
            {
                // Start next tutorial step if no active tasks
                if (activeTasks.Count == 0)
                {
                    CurrentState++;
                    StartTutorialStep(CurrentState);
                }
            }
            
            // Start next queued task if needed
            if (activeTasks.Count == 0 && taskQueue.Count > 0)
            {
                ProcessNextTask();
            }
        }
    }
    
    private void OnDialogueComplete()
    {
        // If we were waiting for dialogue before starting a task
        if (isWaitingForDialogue && pendingStepIndex >= 0)
        {
            isWaitingForDialogue = false;
            CreateTaskForStep(tutorialSteps[pendingStepIndex]);
            pendingStepIndex = -1;
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
        
        // Stop all tasks
        foreach (Task task in activeTasks)
        {
            task.InterruptTask();
        }
        activeTasks.Clear();
        taskQueue.Clear();
        
    }
    
    private void OnDestroy()
    {
        // Unregister from dialogue events
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.OnDialogueSequenceComplete -= OnDialogueComplete;
        }
    }
}