using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSO", menuName = "Dialogue/DialogueNode")]

public class DialogueSO : ScriptableObject
{
    public DialogueLine[] lines;
    public DialogueOption[] options;

    [Header("Conditional Requirements (Optional)")]
    public ActorSO[] requiredNPCs;
    public LocationSO[] requiredLocations;
    public ItemSO[] requiredItems;

    [Header("Control Flags")]
    public bool removeAfterPlay;
    public List<DialogueSO> removeTheseOnPlay;


    public bool isConditionMet()
    {
        if (requiredNPCs.Length > 0)
        {
            foreach (var npc in requiredNPCs)
            {
                if (!GameManager.Instance.DialogueHistoryTracker.HasSpokenWith(npc))
                    return false;
            }
        }

        if (requiredLocations.Length > 0)
        {
            foreach (var location in requiredLocations)
            {
                if (!GameManager.Instance.LocationHistoryTracker.HasVisited(location))
                    return false;
            }
        }

        if (requiredItems.Length > 0)
        {
            foreach (var item in requiredItems)
            {
                if (!InventoryManager.Instance.HasItem(item))
                    return false;
            }
        }
        return true;
    }
}

[System.Serializable]
public class DialogueLine
{
    public ActorSO speaker;
    [TextArea(3, 5)] public string text;

    [Header("Typewriter Settings")]
    public bool useTypewriter = true;
    [Range(0.01f, 0.2f)]
    public float typewriterSpeed = 0.05f;
    public bool instantText = false; // Skip typewriter entirely for dramatic moments

    [Header("Text Effects")]
    [Range(0f, 3f)]
    public float pauseAfterLine = 0f; // Extra pause after this line completes
}

[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public DialogueSO nextDialogue;
}