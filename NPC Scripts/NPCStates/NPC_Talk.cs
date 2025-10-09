using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Talk : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    public Animator interactAnim;
    public List<DialogueSO> conversations;
    private List<DialogueSO> pendingRemovals = new List<DialogueSO>();
    public DialogueSO currentConversation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        anim.Play("Idle");
        interactAnim.Play("Open");
    }

    private void OnDisable()
    {
        interactAnim.Play("Close");
        rb.isKinematic = false;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            if (GameManager.Instance.DialogueManager.isDialogueActive)
            {
                // E key ONLY skips typewriter
                GameManager.Instance.DialogueManager.SkipTypewriter();
            }
            else
            {
                if (GameManager.Instance.DialogueManager.CanStartDialogue())
                {
                    CheckForNewConversation();
                    if (currentConversation != null)
                    {
                        GameManager.Instance.DialogueManager.StartDialogue(currentConversation);
                    }
                }
            }
        }
    }

    public void MarkDialogueForRemoval(DialogueSO convo)
    {
        if (!pendingRemovals.Contains(convo))
            pendingRemovals.Add(convo);
    }

    public void ApplyPendingRemovals()
    {
        foreach (var convo in pendingRemovals)
        {
            conversations.Remove(convo);
        }
        pendingRemovals.Clear();
    }

    private void CheckForNewConversation()
    {
        for (int i = 0; i < conversations.Count; i++)
        {
            var convo = conversations[i];
            if (convo != null && convo.isConditionMet())
            {
                currentConversation = convo;
                if (convo.removeAfterPlay)
                    conversations.RemoveAt(i);

                if (convo.removeAfterPlay && convo.removeTheseOnPlay.Count > 0)
                {
                    foreach (var toRemove in convo.removeTheseOnPlay)
                    {
                        conversations.Remove(toRemove);
                    }
                }
                currentConversation = convo;
                break;
            }
        }
    }

    public void HandleDialogueEnd(DialogueSO dialogue)
    {
        if (dialogue == null) return;

        // Remove this dialogue if flagged
        if (dialogue.removeAfterPlay)
        {
            conversations.Remove(dialogue);
        }

        // Remove other dialogues if specified
        if (dialogue.removeTheseOnPlay != null)
        {
            foreach (var d in dialogue.removeTheseOnPlay)
            {
                conversations.Remove(d);
            }
        }
    }
}