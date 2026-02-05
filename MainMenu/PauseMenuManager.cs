using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;

  

    [Header("Pause Menu Panels")]
    public GameObject pauseMenuPanel;
    public GameObject guidePanel;
    public CanvasGroup pauseCanvasGroup;
    public CanvasGroup guideCanvasGroup;

    [Header("Overlay")]
    public Image overlayImage; // Semi-transparent dark overlay
    [Range(0f, 1f)]
    public float initialOverlayAlpha = 0.0f; // The alpha it starts at when first paused
    public float darkenSpeed = 0.05f;        // How fast it reaches full black (1.0)

    [Header("Fade Transition")]
    public Animator fadeAnimator; // The same fade animator used in SceneChanger
    public float fadeTime = 0.5f;

    [Header("Game UI to Hide (Assign these in Inspector)")]
    public GameObject[] gameUIElements; // All game UI that should be hidden when returning to main menu

    [Header("Audio")]
    public AudioMixerGroup masterMixerGroup; // Optional: for advanced audio control
    [Range(0f, 22000f)]
    public float pausedLowPassFrequency = 800f; // Underwater effect frequency
    [Range(0f, 22000f)]
    public float normalLowPassFrequency = 22000f; // Normal frequency (no filter)
    public float audioTransitionSpeed = 1000f; // How fast the filter transitions

    [Header("Transition Settings")]
    public float panelFadeSpeed = 0.3f;

    private bool isPaused = false;
    private bool isTransitioning = false;
    private AudioLowPassFilter lowPassFilter;
    private AudioSource[] allAudioSources;
    private float targetLowPassFrequency;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("PauseMenuManager initialized and marked as DontDestroyOnLoad");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup low pass filter for underwater effect
        SetupAudioFilter();

        // Initialize UI
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }

        if (guideCanvasGroup != null)
        {
            guideCanvasGroup.alpha = 0;
            guideCanvasGroup.interactable = false;
            guideCanvasGroup.blocksRaycasts = false;
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (guidePanel != null)
            guidePanel.SetActive(false);

        if (overlayImage != null)
        {
            Color overlayColor = overlayImage.color;
            overlayColor.a = 0f;
            overlayImage.color = overlayColor;
        }

        // Start with normal audio
        targetLowPassFrequency = normalLowPassFrequency;
    }

    private void SetupAudioFilter()
    {
        // Add low pass filter to AudioListener (affects all audio in scene)
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener != null)
        {
            lowPassFilter = listener.gameObject.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
            {
                lowPassFilter = listener.gameObject.AddComponent<AudioLowPassFilter>();
            }
            lowPassFilter.cutoffFrequency = normalLowPassFrequency;
            lowPassFilter.lowpassResonanceQ = 1f;
        }
        else
        {
            Debug.LogWarning("PauseMenuManager: No AudioListener found in scene!");
        }
    }

    private void Update()
    {
        // Check for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Block pause during dialogue or shop
            if (IsDialogueOrShopActive())
            {
                Debug.Log("Cannot pause: Dialogue or Shop is active");
                return;
            }

            if (!isPaused)
            {
                Pause();
            }
            else if (!isTransitioning)
            {
                // If in guide, close guide first, then unpause on second ESC
                if (guidePanel != null && guidePanel.activeSelf)
                {
                    HideGuide();
                }
                else
                {
                    Resume();
                }
            }
        }

        if (isPaused && !isTransitioning && overlayImage != null)
        {
            Color currentWeight = overlayImage.color;
            if (currentWeight.a < 1f) // 1f is the equivalent of 255 alpha
            {
                currentWeight.a += darkenSpeed * Time.unscaledDeltaTime;
                overlayImage.color = currentWeight;
            }
        }

        // Smoothly transition audio filter
        if (lowPassFilter != null)
        {
            lowPassFilter.cutoffFrequency = Mathf.MoveTowards(
                lowPassFilter.cutoffFrequency,
                targetLowPassFrequency,
                audioTransitionSpeed * Time.unscaledDeltaTime
            );
        }
    }

    private bool IsDialogueOrShopActive()
    {
        // Check if dialogue is active
        if (DialogueManager.IsDialogueActive)
            return true;

        // Check if shop is active (Time.timeScale = 0 and shop is open)
        if (ShopKeeper.currentShopKeeper != null)
            return true;

        return false;
    }

    public void Pause()
    {
        if (isPaused || isTransitioning) return;

        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            c.a = 0f;
            overlayImage.color = c;
        }

        isPaused = true;
        Time.timeScale = 0f;

        // Apply underwater audio effect
        targetLowPassFrequency = pausedLowPassFrequency;

        // Show pause menu
        StartCoroutine(ShowPauseMenu());

        // Disable player movement
        DisablePlayerControls();

        Debug.Log("Game Paused");
    }

    public void Resume()
    {
        if (!isPaused || isTransitioning) return;

        StartCoroutine(HidePauseMenu(() =>
        {
            isPaused = false;
            Time.timeScale = 1f;

            // Remove underwater audio effect
            targetLowPassFrequency = normalLowPassFrequency;

            // Re-enable player movement
            EnablePlayerControls();

            Debug.Log("Game Resumed");
        }));
    }

    public void ShowGuide()
    {
        if (isTransitioning) return;

        StartCoroutine(ShowGuidePanel());
    }

    public void HideGuide()
    {
        if (isTransitioning) return;

        StartCoroutine(HideGuidePanel());
    }

    public void QuitToMainMenu()
    {
        if (isTransitioning) return;

        StartCoroutine(QuitToMainMenuSequence());
    }

    // === COROUTINES ===

    private IEnumerator ShowPauseMenu()
    {
        isTransitioning = true;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        float elapsed = 0f;

        while (elapsed < panelFadeSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / panelFadeSpeed;

            // Only fade the CanvasGroup (the buttons/text), NOT the overlay alpha
            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            }

            yield return null;
        }

        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }

        isTransitioning = false;
        // Once this is false, your Update() logic below will start darkening the screen.
    }

    private IEnumerator HidePauseMenu(System.Action onComplete = null)
    {
        isTransitioning = true;

        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }

        // Fade out
        float elapsed = 0f;
        Color overlayColor = overlayImage != null ? overlayImage.color : Color.black;
        float startAlpha = overlayColor.a;

        while (elapsed < panelFadeSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / panelFadeSpeed;

            if (overlayImage != null)
            {
                overlayColor.a = Mathf.Lerp(startAlpha, 0f, progress);
                overlayImage.color = overlayColor;
            }

            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            }

            yield return null;
        }

        // Final values
        if (overlayImage != null)
        {
            overlayColor.a = 0f;
            overlayImage.color = overlayColor;
        }

        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        isTransitioning = false;
        onComplete?.Invoke();
    }

    private IEnumerator ShowGuidePanel()
    {
        isTransitioning = true;

        // Fade out pause menu
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.interactable = false;

            float elapsed = 0f;
            while (elapsed < panelFadeSpeed)
            {
                elapsed += Time.unscaledDeltaTime;
                pauseCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / panelFadeSpeed);
                yield return null;
            }
            pauseCanvasGroup.alpha = 0f;
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Fade in guide
        if (guidePanel != null)
            guidePanel.SetActive(true);

        if (guideCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < panelFadeSpeed)
            {
                elapsed += Time.unscaledDeltaTime;
                guideCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / panelFadeSpeed);
                yield return null;
            }
            guideCanvasGroup.alpha = 1f;
            guideCanvasGroup.interactable = true;
            guideCanvasGroup.blocksRaycasts = true;
        }

        isTransitioning = false;
    }

    private IEnumerator HideGuidePanel()
    {
        isTransitioning = true;

        // Fade out guide
        if (guideCanvasGroup != null)
        {
            guideCanvasGroup.interactable = false;
            guideCanvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < panelFadeSpeed)
            {
                elapsed += Time.unscaledDeltaTime;
                guideCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / panelFadeSpeed);
                yield return null;
            }
            guideCanvasGroup.alpha = 0f;
        }

        if (guidePanel != null)
            guidePanel.SetActive(false);

        // Fade in pause menu
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (pauseCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < panelFadeSpeed)
            {
                elapsed += Time.unscaledDeltaTime;
                pauseCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / panelFadeSpeed);
                yield return null;
            }
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }

        isTransitioning = false;
    }

    private IEnumerator QuitToMainMenuSequence()
    {
        isTransitioning = true;

        Debug.Log("=== Starting Quit to Main Menu Sequence ===");

        // STEP 1: Hide all game UI elements first (fade them out)
        yield return StartCoroutine(HideAllGameUI());

        // STEP 2: Stop all audio through AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllAudio();
            Debug.Log("AudioManager: All audio stopped");
        }

        // STEP 3: Play fade out animation if available
        if (fadeAnimator != null)
        {
            fadeAnimator.Play("FadeOut");
            yield return new WaitForSecondsRealtime(fadeTime);
        }

        // STEP 4: Reset time scale BEFORE loading scene
        Time.timeScale = 1f;
        targetLowPassFrequency = normalLowPassFrequency;

        // STEP 5: Reset pause state
        isPaused = false;

        // STEP 6: Hide pause menu panels
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (guidePanel != null)
            guidePanel.SetActive(false);

        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }

        // STEP 7: Reset overlay
        if (overlayImage != null)
        {
            Color overlayColor = overlayImage.color;
            overlayColor.a = 0f;
            overlayImage.color = overlayColor;
        }

        // STEP 8: Re-enable player controls
        EnablePlayerControls();

        Debug.Log("=== Loading Main Menu Scene ===");

        // STEP 9: Load main menu (scene index 0)
        SceneManager.LoadScene(0);

        isTransitioning = false;
    }

    /// <summary>
    /// Fade out and hide all game UI elements before returning to main menu
    /// </summary>
    private IEnumerator HideAllGameUI()
    {
        Debug.Log("Hiding all game UI elements...");

        if (gameUIElements == null || gameUIElements.Length == 0)
        {
            Debug.LogWarning("No game UI elements assigned to hide!");
            yield break;
        }

        // Collect all CanvasGroups
        List<CanvasGroup> canvasGroups = new List<CanvasGroup>();

        foreach (GameObject uiElement in gameUIElements)
        {
            if (uiElement != null && uiElement.activeSelf)
            {
                CanvasGroup cg = uiElement.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    cg = uiElement.AddComponent<CanvasGroup>();
                }
                canvasGroups.Add(cg);
            }
        }

        // Fade out all UI elements simultaneously
        float elapsed = 0f;
        while (elapsed < panelFadeSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / panelFadeSpeed);

            foreach (CanvasGroup cg in canvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = alpha;
                }
            }

            yield return null;
        }

        // Final pass: set alpha to 0 and deactivate
        foreach (GameObject uiElement in gameUIElements)
        {
            if (uiElement != null)
            {
                CanvasGroup cg = uiElement.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                }
                uiElement.SetActive(false);
                Debug.Log($"Hidden UI element: {uiElement.name}");
            }
        }

        Debug.Log("All game UI elements hidden");
    }

    // === PLAYER CONTROL MANAGEMENT ===

    private void DisablePlayerControls()
    {
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.enabled = false;
            player.Stop();
        }
    }

    private void EnablePlayerControls()
    {
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.enabled = true;
        }
    }

    // === SCENE CHANGE HANDLING ===

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-setup audio filter in new scene
        SetupAudioFilter();

        // Reset pause state when loading main menu
        if (scene.buildIndex == 0) // Main menu scene
        {
            isPaused = false;
            Time.timeScale = 1f;
            targetLowPassFrequency = normalLowPassFrequency;

            // Hide all pause menu elements
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
            if (guidePanel != null)
                guidePanel.SetActive(false);

            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.alpha = 0f;
                pauseCanvasGroup.interactable = false;
                pauseCanvasGroup.blocksRaycasts = false;
            }

            if (overlayImage != null)
            {
                Color overlayColor = overlayImage.color;
                overlayColor.a = 0f;
                overlayImage.color = overlayColor;
            }

            Debug.Log("PauseMenuManager: Reset for main menu scene");
        }
        else // Game scene
        {
            // Show all game UI elements when entering game scene
            ShowAllGameUI();
        }
    }

    /// <summary>
    /// Show all game UI elements when entering game scene
    /// </summary>
    private void ShowAllGameUI()
    {
        if (gameUIElements == null || gameUIElements.Length == 0)
            return;

        foreach (GameObject uiElement in gameUIElements)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(true);

                CanvasGroup cg = uiElement.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                }

                Debug.Log($"Showed UI element: {uiElement.name}");
            }
        }

        Debug.Log("All game UI elements shown");
    }

    // === PUBLIC HELPERS ===

    public bool IsPaused()
    {
        return isPaused;
    }
}