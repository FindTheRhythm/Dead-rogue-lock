using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private const float BaseMusicVolume = 0.1f;

    public static MusicManager Instance;

    [Header("Music")]
    [SerializeField] private AudioClip explorationMusic;
    [SerializeField] private AudioClip combatMusic;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip deathMusic;
    [SerializeField] private AudioClip victoryMusic;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;

    [Header("Pause Mix")]
    [SerializeField] private float pauseFadeDuration = 0.25f;
    [SerializeField] private float resumeFadeDuration = 0.4f;

    [Header("Transition")]
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float silenceDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 1f;

    [Header("Start Fade")]
    [SerializeField] private float startFadeInDuration = 3f;

    [Header("Scenes")]
    [SerializeField] private string gameplaySceneName = "Level_1";

    [Header("Low HP Effect")]
    [SerializeField] private float maxSlowPitch = 0.6f;
    [SerializeField] private float pitchLerpSpeed = 2f;

    private AudioClip currentClip;
    private Coroutine transitionRoutine;
    private Coroutine startRoutine;

    private PlayerHealth player;

    private float currentPitch = 1f;
    private bool isOneShotPlaying;
    private AudioMixerSnapshot normalSnapshot;
    private AudioMixerSnapshot pausedSnapshot;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.loop = true;

        AudioMixer mainMixer = Resources.Load<AudioMixer>("Audio/MainMixer");
        if (mainMixer != null)
        {
            normalSnapshot = mainMixer.FindSnapshot("Snapshot");
            pausedSnapshot = mainMixer.FindSnapshot("Paused");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        GamePauseState.OnPauseChanged += OnPauseChanged;
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerHealth>();

        if (SceneManager.GetActiveScene().name != gameplaySceneName)
        {
            StopMusicImmediately();
            return;
        }

        ApplyPauseSnapshot(
            GamePauseState.IsPaused && GamePauseState.ShouldDuckMusic,
            0f
        );

        if (transitionRoutine == null && !musicSource.isPlaying)
            startRoutine = StartCoroutine(StartMusicFadeIn());
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        GamePauseState.OnPauseChanged -= OnPauseChanged;
        Instance = null;
    }

    private void Update()
    {
        ApplyLowHpPitch();
        ApplyVolumeFromSettings();
    }

    private void LateUpdate()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerHealth>();
    }
    
    // =========================
    // VOLUME SYNC (UI SLIDER)
    // =========================

    private void ApplyVolumeFromSettings()
    {
        float targetVolume = GetTargetVolume();

        // не ломаем fade — только если нет transition
        if (transitionRoutine == null)
        {
            musicSource.volume = targetVolume;
        }
    }

    // =========================
    // MUSIC STATES
    // =========================

    public void PlayExploration() => ChangeMusic(explorationMusic);
    public void PlayCombat() => ChangeMusic(combatMusic);
    public void PlayBoss() => ChangeMusic(bossMusic);

    // =========================
    // ONE SHOT
    // =========================

    public void PlayDeath() => PlayOneShot(deathMusic);
    public void PlayVictory() => PlayOneShot(victoryMusic);

    private void OnPauseChanged(bool isPaused)
    {
        float transitionDuration = isPaused
            ? pauseFadeDuration
            : resumeFadeDuration;

        ApplyPauseSnapshot(
            isPaused && GamePauseState.ShouldDuckMusic,
            transitionDuration
        );
    }

    private void ApplyPauseSnapshot(bool isPaused, float transitionDuration)
    {
        AudioMixerSnapshot snapshot = isPaused
            ? pausedSnapshot
            : normalSnapshot;

        if (snapshot != null)
            snapshot.TransitionTo(Mathf.Max(0f, transitionDuration));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = FindFirstObjectByType<PlayerHealth>();

        if (scene.name == gameplaySceneName)
            PlayExploration();
        else
            StopMusicImmediately();
    }

    // =========================
    // SWITCH MUSIC
    // =========================

    private void ChangeMusic(AudioClip newClip)
    {
        if (newClip == null)
            return;

        if (currentClip == newClip && musicSource.isPlaying)
            return;

        currentClip = newClip;
        isOneShotPlaying = false;

        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine(newClip));
    }

    private IEnumerator TransitionRoutine(AudioClip newClip)
    {
        float targetVolume = GetTargetVolume();

        // FADE OUT
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float t = 0f;

            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
                yield return null;
            }

            musicSource.Stop();
        }

        yield return new WaitForSecondsRealtime(silenceDuration);

        // SWITCH
        musicSource.clip = newClip;
        musicSource.loop = true;

        musicSource.volume = 0f;
        musicSource.pitch = 1f;
        currentPitch = 1f;

        musicSource.Play();

        float f = 0f;

        while (f < fadeInDuration)
        {
            f += Time.unscaledDeltaTime;

            float vol =
                Mathf.Lerp(0f, targetVolume, f / fadeInDuration);

            musicSource.volume = vol;

            yield return null;
        }

        musicSource.volume = targetVolume;
        transitionRoutine = null;
    }

    // =========================
    // START FADE
    // =========================

    private IEnumerator StartMusicFadeIn()
    {
        if (explorationMusic == null)
            yield break;

        currentClip = explorationMusic;

        musicSource.clip = explorationMusic;
        musicSource.loop = true;

        musicSource.volume = 0f;
        musicSource.pitch = 1f;
        currentPitch = 1f;

        musicSource.Play();

        float targetVolume = GetTargetVolume();

        float t = 0f;

        while (t < startFadeInDuration)
        {
            t += Time.unscaledDeltaTime;

            float k = Mathf.SmoothStep(0f, 1f, t / startFadeInDuration);

            musicSource.volume = Mathf.Lerp(0f, targetVolume, k);

            yield return null;
        }

        musicSource.volume = targetVolume;
        startRoutine = null;
    }

    // =========================
    // LOW HP PITCH
    // =========================

    private void ApplyLowHpPitch()
    {
        if (player == null || musicSource.clip == null)
            return;

        // ❗ не трогаем death/victory
        if (isOneShotPlaying)
            return;

        float hp = player.CurrentHealth / (float)player.MaxHealth;

        float intensity = 1f - hp;
        intensity = Mathf.SmoothStep(0f, 1f, intensity);

        float targetPitch = Mathf.Lerp(1f, maxSlowPitch, intensity);

        currentPitch = Mathf.Lerp(
            currentPitch,
            targetPitch,
            Time.unscaledDeltaTime * pitchLerpSpeed
        );

        musicSource.pitch = currentPitch;
    }

    // =========================
    // ONE SHOT
    // =========================

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
            return;

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }

        currentClip = clip;

        musicSource.Stop();

        musicSource.loop = false;

        isOneShotPlaying = true; 

        musicSource.pitch = 1f;
        currentPitch = 1f;

        musicSource.clip = clip;
        musicSource.volume = GetTargetVolume();

        musicSource.Play();
    }

    private static float GetTargetVolume()
    {
        return Mathf.Clamp01(GameSettings.MusicVolume) * BaseMusicVolume;
    }

    private void StopMusicImmediately()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.loop = true;
        musicSource.pitch = 1f;
        currentPitch = 1f;
        currentClip = null;
        isOneShotPlaying = false;
    }
}
