using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Video Player")]
    public VideoPlayer introVideoPlayer;
    public float introVideoDuration = 6f; // Only play first 6 seconds

    [Header("Background")]
    public GameObject backgroundImage; // Static background image

    [Header("Audio")]
    public AudioSource backgroundMusic; // Background music that plays after intro
    public float musicVolume = 0.3f; // Target volume for background music

    [Header("UI Elements - Main Menu")]
    public GameObject videoDisplay; // The RawImage showing the intro video
    public GameObject titleText;
    public GameObject buttonContainer;
    public CanvasGroup uiCanvasGroup;

    [Header("UI Elements - How to Play")]
    public GameObject howToPlayPage; // The how to play panel
    public CanvasGroup howToPlayCanvasGroup; // For fade in/out

    [Header("Transition Settings")]
    public float slideDuration = 1.5f; // How long the slide takes
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Smooth easing

    [Header("Settings")]
    public float uiFadeInDuration = 1.5f;
    public float pageFadeDuration = 0.3f; // How long page transitions take
    public float musicFadeInDuration = 1.0f; // How long music takes to fade in
    public float musicFadeOutDuration = 0.5f; // How long music takes to fade out
    public string gameSceneName = "GameScene"; // Change this to your game scene name

    [Header("Debug")]
    public bool skipIntroVideo = false; // Set to true in Inspector to skip video for testing

    // Static flag to track if intro has been shown this session
    private static bool hasPlayedIntroThisSession = false;

    private bool introComplete = false;
    private bool isTransitioning = false; // Prevents double-clicks
    private Coroutine videoTimerCoroutine;
    private RectTransform videoDisplayRect;

    private void Start()
    {
        // Cache RectTransform
        if (videoDisplay != null)
        {
            videoDisplayRect = videoDisplay.GetComponent<RectTransform>();
        }

        // Ensure UI starts hidden
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
        }

        if (titleText != null)
            titleText.SetActive(false);

        if (buttonContainer != null)
            buttonContainer.SetActive(false);

        // Ensure background image starts hidden
        if (backgroundImage != null)
            backgroundImage.SetActive(false);

        // Ensure How to Play page starts hidden
        if (howToPlayPage != null)
        {
            howToPlayPage.SetActive(false);

            // Add CanvasGroup if not present
            if (howToPlayCanvasGroup == null)
            {
                howToPlayCanvasGroup = howToPlayPage.GetComponent<CanvasGroup>();
                if (howToPlayCanvasGroup == null)
                {
                    howToPlayCanvasGroup = howToPlayPage.AddComponent<CanvasGroup>();
                }
            }
        }

        // Ensure music starts silent
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = 0f;
        }

        // Check if intro should be skipped (returning from game or debug mode)
        if (skipIntroVideo || hasPlayedIntroThisSession)
        {
            Debug.Log(hasPlayedIntroThisSession ?
                "Skipping intro video (already played this session)" :
                "Skipping intro video (debug mode)");

            SkipDirectlyToMainMenu();
            return;
        }

        // Mark that intro has been shown for this session
        hasPlayedIntroThisSession = true;

        // Subscribe to video events
        if (introVideoPlayer != null)
        {
            introVideoPlayer.errorReceived += OnVideoError;
            introVideoPlayer.prepareCompleted += OnVideoPrepared;
            introVideoPlayer.started += OnVideoStarted;

            // Prepare the video
            introVideoPlayer.Prepare();
        }
        else
        {
            Debug.LogWarning("No intro video player assigned!");
            OnIntroVideoEnd(null);
        }
    }

    /// <summary>
    /// Skip directly to main menu without playing intro video
    /// Called when returning from game scene or when debug skip is enabled
    /// </summary>
    private void SkipDirectlyToMainMenu()
    {
        introComplete = true;

        // Hide video display immediately
        if (videoDisplay != null)
            videoDisplay.SetActive(false);

        // Show background immediately
        if (backgroundImage != null)
            backgroundImage.SetActive(true);

        // Start playing background music
        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
            StartCoroutine(FadeInMusic());
        }

        // Show UI immediately (or with quick fade)
        StartCoroutine(FadeInUI());
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"Video Error: {message}");
        // Skip to background if video fails
        OnIntroVideoEnd(null);
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("Video prepared successfully");
        // Video is ready, now play it
        introVideoPlayer.Play();
    }

    private void OnVideoStarted(VideoPlayer source)
    {
        Debug.Log($"Video started playing. Total length: {source.length} seconds, playing only {introVideoDuration} seconds");

        // Start a timer to stop the video after the specified duration
        if (videoTimerCoroutine != null)
        {
            StopCoroutine(videoTimerCoroutine);
        }
        videoTimerCoroutine = StartCoroutine(VideoTimer());
    }

    private IEnumerator VideoTimer()
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(introVideoDuration);

        Debug.Log($"Video timer reached {introVideoDuration} seconds, transitioning to background");

        // Stop the video
        if (introVideoPlayer != null)
        {
            introVideoPlayer.Stop();
        }

        // Trigger transition
        OnIntroVideoEnd(introVideoPlayer);
    }

    private void OnIntroVideoEnd(VideoPlayer vp)
    {
        if (introComplete) return;
        introComplete = true;

        Debug.Log("Transitioning from intro video to background with slide effect");

        // Start transition to static background with slide effect
        StartCoroutine(TransitionToBackground());
    }

    private IEnumerator TransitionToBackground()
    {
        // Show background image FIRST (it will be behind the video)
        if (backgroundImage != null)
        {
            backgroundImage.SetActive(true);
        }

        // Start playing background music and fade it in during the slide
        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
            StartCoroutine(FadeInMusic());
        }

        // Slide the video down to reveal the background
        if (videoDisplayRect != null)
        {
            float canvasHeight = videoDisplayRect.rect.height;
            Vector2 startPosition = videoDisplayRect.anchoredPosition;
            Vector2 endPosition = new Vector2(startPosition.x, startPosition.y - canvasHeight);

            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / slideDuration;

                // Use animation curve for smooth easing
                float curvedProgress = slideCurve.Evaluate(progress);

                // Slide the video down
                videoDisplayRect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, curvedProgress);

                yield return null;
            }

            // Ensure final position
            videoDisplayRect.anchoredPosition = endPosition;

            // Now we can hide and reset the video display
            videoDisplay.SetActive(false);
            videoDisplayRect.anchoredPosition = startPosition; // Reset for next time
        }

        // Fade in UI after slide completes
        StartCoroutine(FadeInUI());
    }

    private IEnumerator FadeInMusic()
    {
        if (backgroundMusic == null) yield break;

        float elapsed = 0f;
        while (elapsed < musicFadeInDuration)
        {
            elapsed += Time.deltaTime;
            backgroundMusic.volume = Mathf.Lerp(0f, musicVolume, elapsed / musicFadeInDuration);
            yield return null;
        }

        backgroundMusic.volume = musicVolume;
    }

    private IEnumerator FadeOutMusic()
    {
        if (backgroundMusic == null) yield break;

        float startVolume = backgroundMusic.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeOutDuration);
            yield return null;
        }

        backgroundMusic.volume = 0f;
        backgroundMusic.Stop();
    }

    private IEnumerator FadeInUI()
    {
        // Activate UI elements
        if (titleText != null)
            titleText.SetActive(true);

        if (buttonContainer != null)
            buttonContainer.SetActive(true);

        // Fade in
        float elapsed = 0f;
        while (elapsed < uiFadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (uiCanvasGroup != null)
            {
                uiCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / uiFadeInDuration);
            }
            yield return null;
        }

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 1f;
        }
    }

    // Called by Play button
    public void PlayGame()
    {
        if (isTransitioning) return; // Prevent double-clicks
        isTransitioning = true;

        // Fade out music then load scene
        StartCoroutine(PlayGameSequence());
    }

    private IEnumerator PlayGameSequence()
    {
        // Fade out the music
        yield return StartCoroutine(FadeOutMusic());

        // Load the game scene (index 1 if MainMenu is index 0)
        SceneManager.LoadScene(1);

        // Alternative: Load by name
        // SceneManager.LoadScene(gameSceneName);
    }

    // Called by How to Play button
    public void ShowHowToPlay()
    {
        if (isTransitioning) return; // Prevent multiple clicks

        StartCoroutine(ShowHowToPlayPage());
    }

    private IEnumerator ShowHowToPlayPage()
    {
        isTransitioning = true;

        // Fade out main menu
        if (uiCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < pageFadeDuration)
            {
                elapsed += Time.deltaTime;
                uiCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / pageFadeDuration);
                yield return null;
            }
            uiCanvasGroup.alpha = 0f;
        }

        // Hide main menu elements
        if (titleText != null)
            titleText.SetActive(false);
        if (buttonContainer != null)
            buttonContainer.SetActive(false);

        // Show How to Play page
        if (howToPlayPage != null)
        {
            howToPlayPage.SetActive(true);

            // Fade in How to Play page
            if (howToPlayCanvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < pageFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    howToPlayCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / pageFadeDuration);
                    yield return null;
                }
                howToPlayCanvasGroup.alpha = 1f;
            }
        }

        isTransitioning = false;
    }

    // Called by Back button on How to Play page
    public void HideHowToPlay()
    {
        if (isTransitioning) return; // Prevent multiple clicks

        StartCoroutine(HideHowToPlayPage());
    }

    private IEnumerator HideHowToPlayPage()
    {
        isTransitioning = true;

        // Fade out How to Play page
        if (howToPlayCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < pageFadeDuration)
            {
                elapsed += Time.deltaTime;
                howToPlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / pageFadeDuration);
                yield return null;
            }
            howToPlayCanvasGroup.alpha = 0f;
        }

        // Hide How to Play page
        if (howToPlayPage != null)
            howToPlayPage.SetActive(false);

        // Show main menu elements
        if (titleText != null)
            titleText.SetActive(true);
        if (buttonContainer != null)
            buttonContainer.SetActive(true);

        // Fade in main menu
        if (uiCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < pageFadeDuration)
            {
                elapsed += Time.deltaTime;
                uiCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / pageFadeDuration);
                yield return null;
            }
            uiCanvasGroup.alpha = 1f;
        }

        isTransitioning = false;
    }

    // Called by Quit button
    public void QuitGame()
    {
        if (isTransitioning) return; // Prevent double-clicks
        isTransitioning = true;

        // Fade out music then quit
        StartCoroutine(QuitGameSequence());
    }

    private IEnumerator QuitGameSequence()
    {
        // Fade out the music
        yield return StartCoroutine(FadeOutMusic());

        // Quit the game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (introVideoPlayer != null)
        {
            introVideoPlayer.errorReceived -= OnVideoError;
            introVideoPlayer.prepareCompleted -= OnVideoPrepared;
            introVideoPlayer.started -= OnVideoStarted;
        }

        // Stop any running coroutines
        if (videoTimerCoroutine != null)
        {
            StopCoroutine(videoTimerCoroutine);
        }
    }
}