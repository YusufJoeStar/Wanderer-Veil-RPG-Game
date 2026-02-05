using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource combatMusicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;

    [Header("Audio Data")]
    public AudioData audioData;

    [Header("Music Settings")]
    public float musicFadeTime = 1.5f;
    public float combatMusicFadeTime = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("SFX Cooldown (Prevent Spam)")]
    public float sfxCooldown = 0.05f;

    private Dictionary<string, float> sfxTimers = new Dictionary<string, float>();
    private Coroutine musicFadeCoroutine;
    private Coroutine combatFadeCoroutine;
    private bool isInCombat = false;
    private int enemiesInRange = 0;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize audio sources if not assigned
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (combatMusicSource == null) combatMusicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (ambientSource == null) ambientSource = gameObject.AddComponent<AudioSource>();

        // Configure audio sources
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0f;

        combatMusicSource.loop = true;
        combatMusicSource.playOnAwake = false;
        combatMusicSource.volume = 0f;

        sfxSource.playOnAwake = false;
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Start playing main game music when game loads
        if (audioData != null && audioData.gameMusic != null)
        {
            PlayMusic(audioData.gameMusic, musicSource);
        }
    }

    /// <summary>
    /// Called whenever a scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"AudioManager: Scene loaded - {scene.name} (Build Index: {scene.buildIndex})");

        // Reset combat state when changing scenes
        isInCombat = false;
        enemiesInRange = 0;

        // Check which scene we're in and play appropriate music
        if (scene.buildIndex == 0) // Main Menu scene
        {
            // Main menu scene - don't play game music
            Debug.Log("AudioManager: Main menu detected, not playing game music");
        }
        else if (scene.buildIndex == 1) // Game scene (adjust if needed)
        {
            // Game scene - play game music
            Debug.Log("AudioManager: Game scene detected, playing game music");

            // Only play if not already playing the correct music
            if (musicSource.clip != audioData.gameMusic || !musicSource.isPlaying)
            {
                PlayGameMusic();
            }
        }
        // Add more scene checks here if you have other scenes
    }

    #region Music Control

    public void PlayMusic(AudioClip clip, AudioSource source)
    {
        if (clip == null || source == null) return;

        source.clip = clip;
        source.loop = true; // IMPORTANT: Make sure it loops!
        source.Play();
        StartCoroutine(FadeIn(source, musicFadeTime, musicVolume));
    }

    public void StopMusic(AudioSource source)
    {
        if (source == null) return;
        StartCoroutine(FadeOut(source, musicFadeTime));
    }

    public void PlayGameMusic()
    {
        if (audioData == null || audioData.gameMusic == null) return;

        // Start crossfade back to game music
        StartCoroutine(CrossfadeToGame());
    }

    private IEnumerator CrossfadeToGame()
    {
        // Fade out current music (shop or combat)
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        while (elapsed < musicFadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeTime);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        // Start game music
        musicSource.clip = audioData.gameMusic;
        musicSource.loop = true; // IMPORTANT: Ensure loop is set
        musicSource.Play();

        // Fade in game music
        elapsed = 0f;
        while (elapsed < musicFadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / musicFadeTime);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    public void PlayShopMusic()
    {
        if (audioData == null || audioData.shopMusic == null) return;

        // Fade out game music first
        StartCoroutine(CrossfadeToShop());
    }

    private IEnumerator CrossfadeToShop()
    {
        // Fade out current music
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        while (elapsed < musicFadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeTime);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        // Start shop music
        musicSource.clip = audioData.shopMusic;
        musicSource.loop = true; // IMPORTANT: Ensure loop is set
        musicSource.Play();

        // Fade in shop music
        elapsed = 0f;
        while (elapsed < musicFadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume * 0.8f, elapsed / musicFadeTime);
            yield return null;
        }

        musicSource.volume = musicVolume * 0.8f;
    }

    #endregion

    #region Combat Music System

    public void EnterCombat()
    {
        enemiesInRange++;

        Debug.Log($"EnterCombat called. Enemies in range: {enemiesInRange}");

        if (!isInCombat && enemiesInRange > 0)
        {
            isInCombat = true;
            TransitionToCombatMusic();
        }
    }

    public void ExitCombat()
    {
        enemiesInRange--;

        Debug.Log($"ExitCombat called. Enemies in range: {enemiesInRange}");

        if (isInCombat && enemiesInRange <= 0)
        {
            isInCombat = false;
            enemiesInRange = 0; // Reset to prevent negative
            TransitionToNormalMusic();
        }
    }

    private void TransitionToCombatMusic()
    {
        if (audioData == null || audioData.combatMusic == null) return;

        Debug.Log("Transitioning TO combat music");

        // Stop any ongoing fade coroutines
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        if (combatFadeCoroutine != null) StopCoroutine(combatFadeCoroutine);

        // Start the crossfade
        StartCoroutine(CrossfadeToCombat());
    }

    private IEnumerator CrossfadeToCombat()
    {
        // Fade out normal music
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        while (elapsed < combatMusicFadeTime)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / combatMusicFadeTime);
            yield return null;
        }

        musicSource.volume = 0f;

        // Setup combat music if not already playing
        if (combatMusicSource.clip != audioData.combatMusic)
        {
            combatMusicSource.clip = audioData.combatMusic;
            combatMusicSource.loop = true; // IMPORTANT: Ensure loop is set
        }

        if (!combatMusicSource.isPlaying)
        {
            combatMusicSource.Play();
        }

        // Fade in combat music
        elapsed = 0f;
        while (elapsed < combatMusicFadeTime)
        {
            elapsed += Time.deltaTime;
            combatMusicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / combatMusicFadeTime);
            yield return null;
        }

        combatMusicSource.volume = musicVolume;
    }

    private void TransitionToNormalMusic()
    {
        Debug.Log("Transitioning BACK to normal music");

        // Stop any ongoing fade coroutines
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        if (combatFadeCoroutine != null) StopCoroutine(combatFadeCoroutine);

        // Start the crossfade back
        StartCoroutine(CrossfadeToNormal());
    }

    private IEnumerator CrossfadeToNormal()
    {
        // Fade out combat music
        float elapsed = 0f;
        float startVolume = combatMusicSource.volume;

        while (elapsed < combatMusicFadeTime)
        {
            elapsed += Time.deltaTime;
            combatMusicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / combatMusicFadeTime);
            yield return null;
        }

        combatMusicSource.volume = 0f;
        combatMusicSource.Stop();

        // Setup normal music if not already set
        if (musicSource.clip != audioData.gameMusic)
        {
            musicSource.clip = audioData.gameMusic;
            musicSource.loop = true; // IMPORTANT: Ensure loop is set
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }

        // Fade in normal music
        elapsed = 0f;
        while (elapsed < combatMusicFadeTime)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / combatMusicFadeTime);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    #endregion

    #region SFX Control

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }

    public void PlaySFX(string soundName, float volumeMultiplier = 1f)
    {
        if (audioData == null) return;

        // Check cooldown to prevent spam
        if (sfxTimers.ContainsKey(soundName) && Time.time < sfxTimers[soundName])
            return;

        AudioClip clip = audioData.GetSFX(soundName);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
            sfxTimers[soundName] = Time.time + sfxCooldown;
        }
        else
        {
            // Don't spam warnings, just log once
            if (!sfxTimers.ContainsKey(soundName + "_warning"))
            {
                Debug.LogWarning($"AudioManager: Missing audio clip for '{soundName}'");
                sfxTimers[soundName + "_warning"] = Time.time + 60f; // Only warn once per minute
            }
        }
    }

    public void PlayRandomSFX(AudioClip[] clips, float volumeMultiplier = 1f)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlaySFX(clip, volumeMultiplier);
    }

    #endregion

    #region Fade Coroutines

    private IEnumerator FadeIn(AudioSource source, float duration, float targetVolume)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled for menus
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }

    #endregion

    #region Volume Control

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        combatMusicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    #endregion

    #region Footstep System

    public void PlayFootstep()
    {
        if (audioData != null && audioData.footsteps != null && audioData.footsteps.Length > 0)
        {
            PlayRandomSFX(audioData.footsteps, 0.3f); // Quieter volume for footsteps
        }
    }

    #endregion

    #region Quit to Main Menu

    /// <summary>
    /// Stop all audio immediately when quitting to main menu
    /// Called by PauseMenuManager before loading main menu scene
    /// </summary>
    public void StopAllAudio()
    {
        Debug.Log("AudioManager: Stopping all audio immediately...");

        // Stop all coroutines (fade effects)
        StopAllCoroutines();

        // Stop and reset all audio sources
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.volume = 0f;
            musicSource.clip = null;
        }

        if (combatMusicSource != null)
        {
            combatMusicSource.Stop();
            combatMusicSource.volume = 0f;
            combatMusicSource.clip = null;
        }

        if (sfxSource != null)
        {
            sfxSource.Stop();
        }

        if (ambientSource != null)
        {
            ambientSource.Stop();
            ambientSource.volume = 0f;
        }

        // Reset combat state
        isInCombat = false;
        enemiesInRange = 0;

        Debug.Log("AudioManager: All audio stopped");
    }

    #endregion
}