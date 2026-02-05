using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Add this component to any Enemy you want to persist between scenes.
/// When defeated, it will be marked and won't reappear when you return to the scene.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class PersistentEnemy : MonoBehaviour
{
    [Header("Persistence Settings")]
    [Tooltip("Unique ID for this enemy. Must be unique within the scene!")]
    public string uniqueID;

    [ContextMenu("Generate Unique ID")]
    private void GenerateUniqueID()
    {
        uniqueID = $"Enemy_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        Debug.Log($"Generated unique ID: {uniqueID}");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private EnemyHealth enemyHealth;
    private string currentSceneName;
    private bool hasMarkedAsDefeated = false;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        currentSceneName = SceneManager.GetActiveScene().name;

        // Validate unique ID
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogError($"PersistentEnemy on {gameObject.name} has no unique ID! Right-click and select 'Generate Unique ID'");
        }
    }

    private void OnEnable()
    {
        // Subscribe to enemy death event
        EnemyHealth.OnMonsterDefeated += OnEnemyDefeated;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        EnemyHealth.OnMonsterDefeated -= OnEnemyDefeated;
    }

    /// <summary>
    /// Called when ANY enemy is defeated
    /// </summary>
    private void OnEnemyDefeated(int exp)
    {
        // Check if this is OUR enemy being defeated
        if (!hasMarkedAsDefeated && enemyHealth != null && enemyHealth.currentHealth <= 0)
        {
            MarkAsDefeated();
        }
    }

    /// <summary>
    /// Mark this enemy as defeated in the persistence system
    /// </summary>
    private void MarkAsDefeated()
    {
        if (hasMarkedAsDefeated) return; // Prevent double-marking

        hasMarkedAsDefeated = true;

        if (PersistentSceneData.Instance != null && !string.IsNullOrEmpty(uniqueID))
        {
            PersistentSceneData.Instance.MarkEnemyDefeated(currentSceneName, uniqueID);
            Debug.Log($"Marked enemy as defeated: {currentSceneName}_{uniqueID}");
        }
        else if (PersistentSceneData.Instance == null)
        {
            Debug.LogWarning("PersistentSceneData.Instance is null! Enemy won't persist.");
        }
    }

    /// <summary>
    /// Check if this enemy has already been defeated
    /// </summary>
    public bool IsDefeated()
    {
        if (PersistentSceneData.Instance != null && !string.IsNullOrEmpty(uniqueID))
        {
            return PersistentSceneData.Instance.IsEnemyDefeated(currentSceneName, uniqueID);
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