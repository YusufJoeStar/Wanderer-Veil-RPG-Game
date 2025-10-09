using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{

    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public Image portrait;
    public TMP_Text actorName;
    public TMP_Text dialogueText;
    public Button[] choiceButtons;
    public bool isDialogueActive;
    public PlayerMovement playerMovement;

    [Header("Typewriter Settings")]
    public float defaultTypewriterSpeed = 0.05f;
    public AudioClip typewriterSound;
    [Range(0f, 1f)]
    public float typewriterSoundVolume = 0.3f;

    private DialogueSO currentDialogue;
    private int dialogueIndex;
    private float lastDialogueEndTime;
    private float dialogueCooldown = 0.1f;

    // Typewriter variables
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private string currentFullText = "";
    private AudioSource audioSource;

    // Auto-fit portrait component
    private AutoFitPortrait autoFitPortrait;

    // Static property so other scripts can check if dialogue is active
    public static bool IsDialogueActive { get; private set; }

    private void Awake()
    {
        // Setup audio source for typewriter sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // Setup button listeners and hide them initially
        if (choiceButtons != null)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int buttonIndex = i; // Capture the index for the lambda
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(buttonIndex));
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        // Enable rich text on the dialogue text component
        if (dialogueText != null)
            dialogueText.richText = true;
    }

    public void StartDialogue(DialogueSO dialogueSO)
    {
        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null)
        {
            pm.ForceIdle();
        }

        if (playerMovement != null) playerMovement.enabled = false;

        if (dialogueSO == null || dialogueSO.lines == null || dialogueSO.lines.Length == 0)
        {
            Debug.LogWarning("Invalid dialogue data!");
            return;
        }
        if (Time.unscaledTime - lastDialogueEndTime < dialogueCooldown)
            return;

        currentDialogue = dialogueSO;
        dialogueIndex = 0;
        isDialogueActive = true;
        IsDialogueActive = true;
        HideAllChoices();
        ShowDialogue();
    }

    public void AdvanceDialogue()
    {
        if (currentDialogue == null) return;

        if (dialogueIndex < currentDialogue.lines.Length)
        {
            ShowDialogue();
        }
        else
        {
            // Check if choices are already shown
            bool choicesVisible = false;
            foreach (var button in choiceButtons)
            {
                if (button.gameObject.activeInHierarchy)
                {
                    choicesVisible = true;
                    break;
                }
            }

            if (choicesVisible)
            {
                // Handle choice selection
                HandleChoiceSelection();
            }
            else
            {
                // All lines shown, now show choices or end dialogue
                ShowChoices();
            }
        }
    }

    // Enhanced method for E key - skips typewriter AND advances when needed
    public void SkipTypewriter()
    {
        if (isTyping)
        {
            // If typing, complete the typewriter
            CompleteTypewriter();
        }
        else if (currentDialogue != null && dialogueIndex >= currentDialogue.lines.Length)
        {
            // If all lines are done but choices not shown yet, show choices
            bool choicesVisible = false;
            foreach (var button in choiceButtons)
            {
                if (button.gameObject.activeInHierarchy)
                {
                    choicesVisible = true;
                    break;
                }
            }

            if (!choicesVisible)
            {
                ShowChoices();
            }
        }
    }

    private void HandleChoiceSelection()
    {
        // Get currently selected button
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i].gameObject == selected)
                {
                    OnChoiceSelected(i);
                    return;
                }
            }
        }

        // If no button selected, default to first choice
        OnChoiceSelected(0);
    }

    private void ShowDialogue()
    {
        if (currentDialogue == null || dialogueIndex >= currentDialogue.lines.Length) return;

        DialogueLine line = currentDialogue.lines[dialogueIndex];
        GameManager.Instance.DialogueHistoryTracker.RecordNPC(line.speaker);

        if (line.speaker != null)
        {
            if (portrait != null) portrait.sprite = line.speaker.portrait;
            if (actorName != null) actorName.text = line.speaker.actorName;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // Handle text display with typewriter or instant
        currentFullText = line.text;

        if (line.instantText || !line.useTypewriter)
        {
            // Show text immediately
            dialogueText.text = currentFullText;
            isTyping = false;

            // Still apply pause if specified
            if (line.pauseAfterLine > 0)
            {
                StartCoroutine(PauseAfterLine(line.pauseAfterLine));
            }
        }
        else
        {
            // Start typewriter effect
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);

            typewriterCoroutine = StartCoroutine(TypewriterEffect(line));
        }

        dialogueIndex++;
    }

    private IEnumerator TypewriterEffect(DialogueLine line)
    {
        isTyping = true;
        dialogueText.text = "";

        float speed = line.typewriterSpeed > 0 ? line.typewriterSpeed : defaultTypewriterSpeed;

        // Parse rich text to handle colored text properly
        string displayText = "";
        bool inTag = false;
        string tagBuffer = "";

        for (int i = 0; i < currentFullText.Length; i++)
        {
            char c = currentFullText[i];

            if (c == '<')
            {
                inTag = true;
                tagBuffer = "<";
            }
            else if (c == '>' && inTag)
            {
                inTag = false;
                tagBuffer += ">";
                displayText += tagBuffer;
                dialogueText.text = displayText;
                tagBuffer = "";
                continue; // Don't wait for tags
            }
            else if (inTag)
            {
                tagBuffer += c;
                continue; // Don't display or wait for tag content
            }
            else
            {
                displayText += c;
            }

            dialogueText.text = displayText;

            // Play typing sound (but not for spaces or punctuation)
            if (typewriterSound != null && !inTag && c != ' ')
            {
                audioSource.PlayOneShot(typewriterSound, typewriterSoundVolume);
            }

            // Wait between characters (shorter wait for spaces)
            if (c != ' ')
                yield return new WaitForSeconds(speed);
            else
                yield return new WaitForSeconds(speed * 0.3f);
        }

        isTyping = false;

        // Apply pause after line if specified
        if (line.pauseAfterLine > 0)
        {
            yield return new WaitForSeconds(line.pauseAfterLine);
        }
    }

    private IEnumerator PauseAfterLine(float pauseDuration)
    {
        yield return new WaitForSeconds(pauseDuration);
    }

    private void CompleteTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        dialogueText.text = currentFullText;
        isTyping = false;
    }

    public bool CanStartDialogue()
    {
        return (Time.unscaledTime - lastDialogueEndTime >= dialogueCooldown);
    }

    private void ShowChoices()
    {
        // clear all old listeners before wiring new ones
        foreach (var button in choiceButtons)
        {
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }

        if (currentDialogue.options != null && currentDialogue.options.Length > 0)
        {
            for (int i = 0; i < currentDialogue.options.Length && i < choiceButtons.Length; i++)
            {
                var option = currentDialogue.options[i];
                var button = choiceButtons[i];

                var buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null) buttonText.text = option.optionText;

                int capturedIndex = i; // capture index for listener
                button.onClick.AddListener(() => OnChoiceSelected(capturedIndex));
                button.gameObject.SetActive(true);
            }
        }
        else
        {
            var button = choiceButtons[0];
            button.GetComponentInChildren<TMP_Text>().text = "End";
            button.onClick.AddListener(EndDialogue);
            button.gameObject.SetActive(true);
        }
        EventSystem.current.SetSelectedGameObject(choiceButtons[0].gameObject);
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (currentDialogue?.options != null && choiceIndex < currentDialogue.options.Length)
        {
            var selectedOption = currentDialogue.options[choiceIndex];
            HideAllChoices();

            if (selectedOption.nextDialogue != null)
            {
                // Start the next dialogue
                StartDialogue(selectedOption.nextDialogue);
            }
            else
            {
                // No next dialogue, end conversation
                EndDialogue();
            }
        }
    }

    private void HideAllChoices()
    {
        if (choiceButtons != null)
        {
            foreach (var button in choiceButtons)
            {
                if (button != null) button.gameObject.SetActive(false);
            }
        }
    }

    private void EndDialogue()
    {
        // Stop any ongoing typewriter effects
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (playerMovement != null) playerMovement.enabled = true;
        dialogueIndex = 0;
        isDialogueActive = false;
        IsDialogueActive = false;
        isTyping = false;
        HideAllChoices();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        lastDialogueEndTime = Time.unscaledTime;

        // Handle removal properly
        if (currentDialogue != null)
        {
            // Remove this one if flagged
            if (currentDialogue.removeAfterPlay)
            {
                FindObjectOfType<NPC_Talk>().MarkDialogueForRemoval(currentDialogue);
            }

            // Remove linked dialogues
            if (currentDialogue.removeTheseOnPlay != null)
            {
                foreach (var d in currentDialogue.removeTheseOnPlay)
                {
                    FindObjectOfType<NPC_Talk>().MarkDialogueForRemoval(d);
                }
            }
        }

        // Now actually apply removals
        FindObjectOfType<NPC_Talk>().ApplyPendingRemovals();

        currentDialogue = null;
    }
}