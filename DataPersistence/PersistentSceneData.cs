using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which items have been collected and which enemies have been defeated per scene.
/// This data persists across scene loads during the game session.
/// </summary>
public class PersistentSceneData : MonoBehaviour
{
    public static PersistentSceneData Instance;

    // Track collected items per scene
    // Key = "SceneName_ItemID", Value = true if collected
    private Dictionary<string, bool> collectedItems = new Dictionary<string, bool>();

    // Track defeated enemies per scene
    // Key = "SceneName_EnemyID", Value = true if defeated
    private Dictionary<string, bool> defeatedEnemies = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("PersistentSceneData initialized and marked as DontDestroyOnLoad");
    }

    /// <summary>
    /// Mark an item as collected in the current scene
    /// </summary>
    public void MarkItemCollected(string sceneName, string itemID)
    {
        string key = $"{sceneName}_{itemID}";
        if (!collectedItems.ContainsKey(key))
        {
            collectedItems.Add(key, true);
            Debug.Log($"Item collected and tracked: {key}");
        }
    }

    /// <summary>
    /// Check if an item has been collected in a specific scene
    /// </summary>
    public bool IsItemCollected(string sceneName, string itemID)
    {
        string key = $"{sceneName}_{itemID}";
        return collectedItems.ContainsKey(key) && collectedItems[key];
    }

    /// <summary>
    /// Mark an enemy as defeated in the current scene
    /// </summary>
    public void MarkEnemyDefeated(string sceneName, string enemyID)
    {
        string key = $"{sceneName}_{enemyID}";
        if (!defeatedEnemies.ContainsKey(key))
        {
            defeatedEnemies.Add(key, true);
            Debug.Log($"Enemy defeated and tracked: {key}");
        }
    }

    /// <summary>
    /// Check if an enemy has been defeated in a specific scene
    /// </summary>
    public bool IsEnemyDefeated(string sceneName, string enemyID)
    {
        string key = $"{sceneName}_{enemyID}";
        return defeatedEnemies.ContainsKey(key) && defeatedEnemies[key];
    }

    /// <summary>
    /// Clear all tracked data (useful for new game / reset)
    /// </summary>
    public void ClearAllData()
    {
        collectedItems.Clear();
        defeatedEnemies.Clear();
        Debug.Log("All persistent scene data cleared");
    }

    /// <summary>
    /// Debug: Print all tracked items
    /// </summary>
    [ContextMenu("Debug: Show Collected Items")]
    public void DebugShowCollectedItems()
    {
        Debug.Log($"=== COLLECTED ITEMS ({collectedItems.Count}) ===");
        foreach (var item in collectedItems)
        {
            Debug.Log($"  - {item.Key}");
        }
    }

    /// <summary>
    /// Debug: Print all tracked enemies
    /// </summary>
    [ContextMenu("Debug: Show Defeated Enemies")]
    public void DebugShowDefeatedEnemies()
    {
        Debug.Log($"=== DEFEATED ENEMIES ({defeatedEnemies.Count}) ===");
        foreach (var enemy in defeatedEnemies)
        {
            Debug.Log($"  - {enemy.Key}");
        }
    }
}