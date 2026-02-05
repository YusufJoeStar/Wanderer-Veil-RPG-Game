using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically runs when any scene loads to:
/// 1. Reconnect GameManager references
/// 2. Clean up duplicate managers from DontDestroyOnLoad
/// 3. Hide/show items and enemies based on persistence data
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    private void Awake()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Initialize the current scene immediately
        InitializeScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        StartCoroutine(InitializeSceneDelayed(scene.name));
    }

    private IEnumerator InitializeSceneDelayed(string sceneName)
    {
        // Wait one frame to ensure all objects are instantiated
        yield return null;

        InitializeScene(sceneName);
    }

    private void InitializeScene(string sceneName)
    {
        Debug.Log($"Initializing scene: {sceneName}");

        // Step 1: Reconnect GameManager references
        ReconnectGameManagerReferences();

        // Step 2: Handle items persistence
        HandleItemsPersistence(sceneName);

        // Step 3: Handle enemies persistence
        HandleEnemiesPersistence(sceneName);

        Debug.Log($"Scene initialization complete for: {sceneName}");
    }

    /// <summary>
    /// Finds and reconnects all manager references in GameManager
    /// </summary>
    private void ReconnectGameManagerReferences()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null! Make sure GameManager exists in the first scene.");
            return;
        }

        // Find DialogueManager (should be in DontDestroyOnLoad)
        if (GameManager.Instance.DialogueManager == null)
        {
            GameManager.Instance.DialogueManager = FindObjectOfType<DialogueManager>();
            if (GameManager.Instance.DialogueManager != null)
            {
                Debug.Log("DialogueManager reconnected to GameManager");
            }
            else
            {
                Debug.LogWarning("DialogueManager not found in scene!");
            }
        }

        // Find DialogueHistoryTracker (should be in DontDestroyOnLoad)
        if (GameManager.Instance.DialogueHistoryTracker == null)
        {
            GameManager.Instance.DialogueHistoryTracker = FindObjectOfType<DialogueHistoryTracker>();
            if (GameManager.Instance.DialogueHistoryTracker != null)
            {
                Debug.Log("DialogueHistoryTracker reconnected to GameManager");
            }
        }

        // Find LocationHistoryTracker (should be in DontDestroyOnLoad)
        if (GameManager.Instance.LocationHistoryTracker == null)
        {
            GameManager.Instance.LocationHistoryTracker = FindObjectOfType<LocationHistoryTracker>();
            if (GameManager.Instance.LocationHistoryTracker != null)
            {
                Debug.Log("LocationHistoryTracker reconnected to GameManager");
            }
        }
    }

    /// <summary>
    /// Hide items that have already been collected in this scene
    /// </summary>
    private void HandleItemsPersistence(string sceneName)
    {
        if (PersistentSceneData.Instance == null)
        {
            Debug.LogWarning("PersistentSceneData.Instance is null! Items won't persist.");
            return;
        }

        // Find all PersistentLoot objects in the scene
        PersistentLoot[] allLoot = FindObjectsOfType<PersistentLoot>();

        int hiddenCount = 0;
        foreach (PersistentLoot loot in allLoot)
        {
            if (PersistentSceneData.Instance.IsItemCollected(sceneName, loot.uniqueID))
            {
                loot.gameObject.SetActive(false);
                hiddenCount++;
            }
        }

        if (hiddenCount > 0)
        {
            Debug.Log($"Hidden {hiddenCount} already-collected items in {sceneName}");
        }
    }

    /// <summary>
    /// Hide enemies that have already been defeated in this scene
    /// </summary>
    private void HandleEnemiesPersistence(string sceneName)
    {
        if (PersistentSceneData.Instance == null)
        {
            Debug.LogWarning("PersistentSceneData.Instance is null! Enemies won't persist.");
            return;
        }

        // Find all PersistentEnemy objects in the scene
        PersistentEnemy[] allEnemies = FindObjectsOfType<PersistentEnemy>();

        int hiddenCount = 0;
        foreach (PersistentEnemy enemy in allEnemies)
        {
            if (PersistentSceneData.Instance.IsEnemyDefeated(sceneName, enemy.uniqueID))
            {
                enemy.gameObject.SetActive(false);
                hiddenCount++;
            }
        }

        if (hiddenCount > 0)
        {
            Debug.Log($"Hidden {hiddenCount} already-defeated enemies in {sceneName}");
        }
    }
}