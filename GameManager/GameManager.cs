using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject PauseMenuCanvas;
    public DialogueManager DialogueManager;
    public DialogueHistoryTracker DialogueHistoryTracker;
    public LocationHistoryTracker LocationHistoryTracker;

    [Header("Persistent Objects")]
    public GameObject[] persistentObjects;

    [Header("Cached References")]
    public Camera shopCamera;
    public CanvasGroup canvasGroup;
    public ShopManager shopManager;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Duplicate GameManager found - cleaning up and destroying");
            CleanUpAndDestroy();
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager initialized and marked as DontDestroyOnLoad");
        }
    }

    private void Start()
    {
        MarkPersistentObjects();
        ValidateReferences();
    }

    private void MarkPersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
        {
            if (obj != null)
            {
                DontDestroyOnLoad(obj);
                Debug.Log($"Marked as persistent: {obj.name}");
            }
        }
    }

  
    private void ValidateReferences()
    {
        if (DialogueManager == null)
            Debug.LogWarning("GameManager: DialogueManager reference is null!");

        if (DialogueHistoryTracker == null)
            Debug.LogWarning("GameManager: DialogueHistoryTracker reference is null!");

        if (LocationHistoryTracker == null)
            Debug.LogWarning("GameManager: LocationHistoryTracker reference is null!");
    }

   
    public void ReconnectReferences()
    {
        if (DialogueManager == null)
        {
            DialogueManager = FindObjectOfType<DialogueManager>();
            if (DialogueManager != null)
                Debug.Log("DialogueManager reconnected");
        }

        if (DialogueHistoryTracker == null)
        {
            DialogueHistoryTracker = FindObjectOfType<DialogueHistoryTracker>();
            if (DialogueHistoryTracker != null)
                Debug.Log("DialogueHistoryTracker reconnected");
        }

        if (LocationHistoryTracker == null)
        {
            LocationHistoryTracker = FindObjectOfType<LocationHistoryTracker>();
            if (LocationHistoryTracker != null)
                Debug.Log("LocationHistoryTracker reconnected");
        }
    }

    private void CleanUpAndDestroy()
    {
        foreach (GameObject obj in persistentObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        Destroy(gameObject);
    }
}
