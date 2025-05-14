using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    // Singleton pattern
    public static DialogueSystem Instance { get; private set; }

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float autoAdvanceDelay = 2f;
    [SerializeField] private AudioSource typingSoundSource;
    [SerializeField] private AudioClip typingSound;
    
    private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();
    private bool isDialogueActive = false;
    private Coroutine displayLineCoroutine;
    
    // Event when dialogue finishes
    public event Action OnDialogueSequenceComplete;
    
    // Dialogue entry structure
    [Serializable]
    public class DialogueEntry
    {
        public string text;
        public float displayTime = 3f; // How long to show after typing finishes
        public bool waitForInput = false; // If true, requires input to advance
        public AudioClip voiceClip;
        public string speakerName;
        
        public DialogueEntry(string text, float displayTime = 3f, bool waitForInput = false)
        {
            this.text = text;
            this.displayTime = displayTime;
            this.waitForInput = waitForInput;
        }
    }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Hide dialogue panel at start
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Check for input to advance dialogue
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceDialogue();
        }
    }
    
    // Start a dialogue sequence with multiple entries
    public void StartDialogueSequence(List<DialogueEntry> dialogueEntries)
    {
        // Add all entries to queue
        dialogueQueue.Clear();
        foreach (var entry in dialogueEntries)
        {
            dialogueQueue.Enqueue(entry);
        }
        
        // Start the dialogue sequence
        StartNextDialogue();
    }
    
    // Add a single dialogue entry
    public void AddDialogueEntry(string text, float displayTime = 3f, bool waitForInput = false)
    {
        DialogueEntry entry = new DialogueEntry(text, displayTime, waitForInput);
        dialogueQueue.Enqueue(entry);
        
        // If no dialogue is active, start it
        if (!isDialogueActive)
        {
            StartNextDialogue();
        }
    }
    
    // Start displaying the next dialogue in the queue
    private void StartNextDialogue()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogueSequence();
            return;
        }
        
        // Get the next dialogue
        DialogueEntry currentDialogue = dialogueQueue.Dequeue();
        
        // Show the dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        // Start typing effect
        isDialogueActive = true;
        if (displayLineCoroutine != null)
        {
            StopCoroutine(displayLineCoroutine);
        }
        displayLineCoroutine = StartCoroutine(TypeDialogue(currentDialogue));
    }
    
    // Typing effect coroutine
    private IEnumerator TypeDialogue(DialogueEntry dialogue)
    {
        dialogueText.text = "";
        
        // Play voice clip if available
        if (dialogue.voiceClip != null && typingSoundSource != null)
        {
            typingSoundSource.clip = dialogue.voiceClip;
            typingSoundSource.Play();
        }
        
        // Type each character
        foreach (char letter in dialogue.text.ToCharArray())
        {
            dialogueText.text += letter;
            
            // Play typing sound
            if (typingSound != null && typingSoundSource != null)
            {
                typingSoundSource.PlayOneShot(typingSound, 0.5f);
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        // Wait for input or auto-advance
        if (dialogue.waitForInput)
        {
            // Wait for input handled in Update
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(dialogue.displayTime);
            AdvanceDialogue();
        }
    }
    
    // Advance to the next dialogue or end the sequence
    private void AdvanceDialogue()
    {
        // Skip current typing if still typing
        if (displayLineCoroutine != null)
        {
            StopCoroutine(displayLineCoroutine);
            displayLineCoroutine = null;
            
            // Show full text immediately
            if (dialogueQueue.Count > 0)
            {
                dialogueText.text = dialogueQueue.Peek().text;
                return;
            }
        }
        
        // Move to next dialogue
        StartNextDialogue();
    }
    
    // End the dialogue sequence
    private void EndDialogueSequence()
    {
        isDialogueActive = false;
        
        // Hide the dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        // Invoke completion event
        OnDialogueSequenceComplete?.Invoke();
    }
    
    // Skip the entire dialogue sequence
    public void SkipDialogueSequence()
    {
        if (displayLineCoroutine != null)
        {
            StopCoroutine(displayLineCoroutine);
            displayLineCoroutine = null;
        }
        
        dialogueQueue.Clear();
        EndDialogueSequence();
    }
}