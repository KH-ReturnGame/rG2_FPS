using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public class DialogData
{
    public string speakerName;
    public string dialogText;
    public float displayDuration = 3.0f;
    public AudioClip voiceClip;
    
    // 타이핑 효과 관련 설정
    public bool overrideTypewriterSettings = false;
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.05f;
}

public class DialogEvent : UnityEngine.Events.UnityEvent<bool> { }

public class DialogManager : MonoBehaviour
{
    // Singleton pattern
    public static DialogManager Instance { get; private set; }

    // Dialog UI components
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogContentText;
    
    // Audio source for voice clips
    [SerializeField] private AudioSource voiceAudioSource;
    
    // Typewriter effect
    [Header("Typewriter Effect Settings")]
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private bool canSkipTypewriter = true;
    [SerializeField] private AudioSource typingSoundSource;
    [SerializeField] private AudioClip typingSound;
    
    // Input settings
    [Header("Input Settings")]
    private KeyCode skipDialogKey = KeyCode.F;
    [SerializeField] private bool skipOnMouseClick = true;
    [SerializeField] private float inputCooldown = 0.2f; // 연속 입력 방지
    
    // Dialog handling
    private Queue<DialogData> dialogQueue = new Queue<DialogData>();
    private DialogData currentDialog; // 현재 표시 중인 대화 저장
    private bool isDisplayingDialog = false;
    private bool isTypingEffect = false; // 현재 타이핑 효과 진행 중인지
    private Coroutine activeDialogCoroutine;
    private Coroutine typewriterCoroutine;
    private float lastInputTime = 0;
    
    // Event fired when dialog starts and ends
    public DialogEvent onDialogStateChanged = new DialogEvent();
    
    // Property to check dialog state
    public bool IsDialogActive => isDisplayingDialog;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Initially hide dialog panel
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        // Create audio source if not assigned
        if (voiceAudioSource == null)
        {
            voiceAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Create typewriter sound source if needed
        if (typingSoundSource == null && typingSound != null)
        {
            typingSoundSource = gameObject.AddComponent<AudioSource>();
            typingSoundSource.volume = 0.5f;
        }
    }
    
    private void Update()
    {
        // 대화가 표시 중일 때만 입력 처리
        if (isDisplayingDialog)
        {
            // 쿨다운 체크하여 너무 빠른 연속 입력 방지
            if (Time.time - lastInputTime >= inputCooldown)
            {
                // 키보드 입력 처리
                if (Input.GetKeyDown(skipDialogKey))
                {
                    HandleSkipInput();
                }
                
                // 마우스 클릭 입력 처리
                if (skipOnMouseClick && Input.GetMouseButtonDown(0))
                {
                    HandleSkipInput();
                }
            }
        }
    }
    
    private void HandleSkipInput()
    {
        lastInputTime = Time.time; // 쿨다운 타이머 갱신
        
        // 타이핑 효과가 실행 중이면 (그리고 스킵 가능하면)
        if (isTypingEffect && canSkipTypewriter)
        {
            // 타이핑 효과만 스킵하고 전체 텍스트 표시
            CompleteTyingEffect();
        }
        // 타이핑 효과가 완료된 상태면 다음 대화로 진행
        else
        {
            SkipCurrentDialog();
        }
    }
    
    // 타이핑 효과를 완료하고 전체 텍스트 표시
    private void CompleteTyingEffect()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        isTypingEffect = false;
        
        // 현재 대화의 전체 텍스트를 바로 표시
        if (currentDialog != null)
        {
            dialogContentText.gameObject.SetActive(false);
            dialogContentText.text = currentDialog.dialogText;
            dialogContentText.gameObject.SetActive(true);
            
        }
    }
    
    public void EnqueueDialog(DialogData dialog)
    {
        dialogQueue.Enqueue(dialog);
        
        // Start processing if not already doing so
        if (!isDisplayingDialog && dialogQueue.Count == 1)
        {
            ProcessNextDialog();
        }
    }
    
    public void EnqueueDialogs(DialogData[] dialogs)
    {
        foreach (DialogData dialog in dialogs)
        {
            dialogQueue.Enqueue(dialog);
        }
        
        // Start processing if not already doing so
        if (!isDisplayingDialog && dialogQueue.Count > 0)
        {
            ProcessNextDialog();
        }
    }
    
    private void ProcessNextDialog()
    {
        if (dialogQueue.Count == 0)
        {
            isDisplayingDialog = false;
            isTypingEffect = false;
            currentDialog = null;
            dialogPanel.SetActive(false);
            onDialogStateChanged.Invoke(false);
            return;
        }
        
        isDisplayingDialog = true;
        onDialogStateChanged.Invoke(true);
        
        // 현재 대화 업데이트
        currentDialog = dialogQueue.Dequeue();
        
        // Update UI
        dialogPanel.SetActive(true);
        speakerNameText.text = currentDialog.speakerName;
        
        // 타이핑 효과 설정 결정 (대화별 설정이 있는 경우 우선 적용)
        bool useTypewriterForThisDialog = useTypewriterEffect;
        float typewriterSpeedForThisDialog = typewriterSpeed;

        if (currentDialog.overrideTypewriterSettings)
        {
            useTypewriterForThisDialog = currentDialog.useTypewriterEffect;
            typewriterSpeedForThisDialog = currentDialog.typewriterSpeed;
        }
        
        // 타이핑 효과 사용 결정
        if (useTypewriterForThisDialog)
        {
            dialogContentText.text = ""; // 텍스트 초기화
            isTypingEffect = true;
            
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            typewriterCoroutine = StartCoroutine(TypewriterEffect(currentDialog.dialogText, typewriterSpeedForThisDialog));
        }
        else
        {
            isTypingEffect = false;
            dialogContentText.text = currentDialog.dialogText;
        }
        
        // Play voice clip if available
        if (currentDialog.voiceClip != null)
        {
            voiceAudioSource.clip = currentDialog.voiceClip;
            voiceAudioSource.Play();
        }
        
        // Start coroutine to handle dialog timing
        if (activeDialogCoroutine != null)
        {
            StopCoroutine(activeDialogCoroutine);
        }
        
        // Duration either based on voice clip or default duration
        float duration = currentDialog.voiceClip != null 
            ? currentDialog.voiceClip.length 
            : currentDialog.displayDuration;
            
        activeDialogCoroutine = StartCoroutine(DialogDisplayCoroutine(duration));
    }
    
    private IEnumerator TypewriterEffect(string textToType, float speed)
    {
        dialogContentText.text = "";
        
        for (int i = 0; i < textToType.Length; i++)
        {
            dialogContentText.text += textToType[i];
            
            // 타이핑 소리 재생
            if (typingSound != null && typingSoundSource != null)
            {
                // 공백이 아닐 경우에만 소리 재생
                if (textToType[i] != ' ' && textToType[i] != '\n')
                {
                    typingSoundSource.PlayOneShot(typingSound);
                }
            }
            
            yield return new WaitForSeconds(speed);
        }
        
        // 타이핑 효과 완료
        isTypingEffect = false;
    }
    
    private IEnumerator DialogDisplayCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        // Process next dialog in queue
        ProcessNextDialog();
    }
    
    public void SkipCurrentDialog()
    {
        if (isDisplayingDialog)
        {
            // 타이핑 효과가 진행 중이면 중단
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
                isTypingEffect = false;
            }
            
            // 대화 타이머 중단
            if (activeDialogCoroutine != null)
            {
                StopCoroutine(activeDialogCoroutine);
            }
            
            // 음성이 재생 중이면 중단
            if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.Stop();
            }
            
            // 다음 대화로 넘어가기
            ProcessNextDialog();
        }
    }
    
    public void ClearAllDialogs()
    {
        dialogQueue.Clear();
        currentDialog = null;
        
        if (activeDialogCoroutine != null)
        {
            StopCoroutine(activeDialogCoroutine);
        }
        
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        isTypingEffect = false;
        
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
        }
        
        isDisplayingDialog = false;
        dialogPanel.SetActive(false);
        onDialogStateChanged.Invoke(false);
    }
    
    // 타이핑 효과 설정 메서드
    public void SetTypewriterEffect(bool enabled)
    {
        useTypewriterEffect = enabled;
    }
    
    public void SetTypewriterSpeed(float speed)
    {
        typewriterSpeed = Mathf.Max(0.01f, speed); // 너무 빠른 속도 방지
    }
    
    public void SetTypewriterSkippable(bool skippable)
    {
        canSkipTypewriter = skippable;
    }
}