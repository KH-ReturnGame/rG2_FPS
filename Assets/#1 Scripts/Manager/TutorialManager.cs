using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointEvent : UnityEngine.Events.UnityEvent<GameObject, int> { }

[Serializable]
public class DialogSettings
{
    public bool includeDialog = false;
    [TextArea(2, 5)]
    public string speakerName;
    [TextArea(3, 10)]
    public string dialogText;
    public AudioClip dialogVoiceClip;
    public float dialogDuration = 3.0f;
    
    // 타이핑 효과 관련 설정
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
        public string taskType; // "move", "interact", "wait", "dialog"
        public float taskParameter; // distance for move, seconds for wait
        
        // Dialog Settings
        public DialogSettings dialogSettings = new DialogSettings();
    }
    
    [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    
    // UI References (optional)
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] public TMPro.TextMeshProUGUI instructionText;
    
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
            if (currentTask.CheckCondition())
            {
                ProcessNextTask();
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
        
        // Create dialog data if this step has dialog
        DialogData[] stepDialogs = null;
        if (step.dialogSettings.includeDialog)
        {
            DialogData dialog = new DialogData
            {
                speakerName = step.dialogSettings.speakerName,
                dialogText = step.dialogSettings.dialogText,
                displayDuration = step.dialogSettings.dialogDuration,
                voiceClip = step.dialogSettings.dialogVoiceClip,
                
                // 타이핑 효과 설정 추가
                overrideTypewriterSettings = step.dialogSettings.overrideTypewriterSettings,
                useTypewriterEffect = step.dialogSettings.useTypewriterEffect,
                typewriterSpeed = step.dialogSettings.typewriterSpeed
            };
            stepDialogs = new DialogData[] { dialog };
        }
        
        // Create appropriate task based on step type
        switch (step.taskType.ToLower())
        {
            case "move":
                if (step.targetObject != null)
                {
                    if (step.dialogSettings.includeDialog)
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
                if (step.dialogSettings.includeDialog)
                {
                    AddTask(new WaitForSecondsWithDialogTask(step.taskParameter, stepDialogs));
                }
                else
                {
                    AddTask(new WaitForSecondsTask(step.taskParameter));
                }
                break;
                
            case "dialog":
                if (step.dialogSettings.includeDialog)
                {
                    AddTask(new ListenToDialogTask(stepDialogs));
                }
                else
                {
                    Debug.LogError($"TutorialManager: Dialog task type specified but no dialog configured in step {stepIndex}");
                }
                break;
                
            default:
                Debug.LogError($"TutorialManager: Unknown task type '{step.taskType}' in step {stepIndex}");
                break;
        }
        
        // Start first task if none is running
        if (currentTask == null && taskQueue.Count > 0)
        {
            ProcessNextTask();
        }
    }
    
    public void AddTask(Task task)
    {
        taskQueue.Enqueue(task);
    }
    
    private void ProcessNextTask()
    {
        // End current task if it exists
        if (currentTask != null && currentTask.IsRunning)
        {
            currentTask.EndTask();
        }
    
        // Start next task if available
        if (taskQueue.Count > 0)
        {
            currentTask = taskQueue.Dequeue();
            currentTask.StartTask();
        }
        else
        {
            currentTask = null;
        
            // 마지막 step인지 확인
            if (CurrentState >= tutorialSteps.Count - 1)
            {
                CompleteTutorial();
                return;
            }
        
            // Check if we should advance to next tutorial step
            if (tutorialSteps[CurrentState + 1].requiredCheckpoint == CurrentCheckpoint)
            {
                CurrentState++;
                StartTutorialStep(CurrentState);
            }
        }
    }
    
    public void CheckNextStep(GameObject sender, int checkpointNumber)
    {
        Debug.Log($"Checkpoint triggered: {checkpointNumber}, Current CP: {CurrentCheckpoint}");
        
        // Update checkpoint progress
        if (checkpointNumber == CurrentCheckpoint)
        {
            CurrentCheckpoint++;
            
            // Check if this checkpoint should trigger next tutorial step
            if (CurrentState < tutorialSteps.Count && tutorialSteps[CurrentState].requiredCheckpoint <= CurrentCheckpoint)
            {
                // Interrupt current task if it exists
                if (currentTask != null && currentTask.IsRunning)
                {
                    currentTask.InterruptTask();
                }
                
                // Clear task queue
                taskQueue.Clear();
                
                // Start next tutorial step
                CurrentState++;
                StartTutorialStep(CurrentState);
            }
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
    
    // 대화를 시작하는 도우미 메서드
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
    
    // 여러 대화를 시작하는 도우미 메서드
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
    
    // 현재 실행 중인 대화를 건너뛰는 메서드
    public void SkipCurrentDialog()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.SkipCurrentDialog();
        }
    }
    
    // 모든 대화를 취소하는 메서드
    public void ClearAllDialogs()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.ClearAllDialogs();
        }
    }
}