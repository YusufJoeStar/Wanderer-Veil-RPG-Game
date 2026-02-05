using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Add this component to any Loot object you want to persist between scenes.
/// When collected, it will be marked and won't reappear when you return to the scene.
/// </summary>
[RequireComponent(typeof(Loot))]
public class PersistentLoot : MonoBehaviour
{
    [Header("Persistence Settings")]
    [Tooltip("Unique ID for this loot item. Must be unique within the scene!")]
    public string uniqueID;

    [ContextMenu("Generate Unique ID")]
    private void GenerateUniqueID()
    {
        uniqueID = $"Loot_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        Debug.Log($"Generated unique ID: {uniqueID}");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private Loot lootComponent;
    private string currentSceneName;

    private void Awake()
    {
        lootComponent = GetComponent<Loot>();
        currentSceneName = SceneManager.GetActiveScene().name;

        // Validate unique ID
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogError($"PersistentLoot on {gameObject.name} has no unique ID! Right-click and select 'Generate Unique ID'");
        }
    }

    private void OnEnable()
    {
        // Subscribe to the loot pickup event
        Loot.OnItemLooted += OnItemPickedUp;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        Loot.OnItemLooted -= OnItemPickedUp;
    }

    /// <summary>
    /// Called when ANY loot is picked up - we check if it's THIS loot
    /// </summary>
    private void OnItemPickedUp(ItemSO itemSO, int quantity)
    {
        // Check if this is our loot being picked up
        if (lootComponent != null && lootComponent.itemSO == itemSO)
        {
            // Only mark if this specific instance is being picked up
            // We check if the game object is about to be destroyed
            if (this != null && gameObject != null)
            {
                MarkAsCollected();
            }
        }
    }

    /// <summary>
    /// Mark this item as collected in the persistence system
    /// </summary>
    private void MarkAsCollected()
    {
        if (PersistentSceneData.Instance != null && !string.IsNullOrEmpty(uniqueID))
        {
            PersistentSceneData.Instance.MarkItemCollected(currentSceneName, uniqueID);
            Debug.Log($"Marked loot as collected: {currentSceneName}_{uniqueID}");
        }
        else if (PersistentSceneData.Instance == null)
        {
            Debug.LogWarning("PersistentSceneData.Instance is null! Item won't persist.");
        }
    }

    /// <summary>
    /// Check if this item has already been collected
    /// </summary>
    public bool IsCollected()
    {
        if (PersistentSceneData.Instance != null && !string.IsNullOrEmpty(uniqueID))
        {
            return PersistentSceneData.Instance.IsItemCollected(currentSceneName, uniqueID);
        }
        return false;
    }

    // Editor helper to show status in inspector
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            // Auto-generate ID if missing (editor only)
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GenerateUniqueID();
            }
#endif
        }
    }
}