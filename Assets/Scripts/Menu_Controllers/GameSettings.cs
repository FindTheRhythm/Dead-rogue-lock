using System.Collections;
using UnityEngine;

public static class GameSettings
{
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string FULLSCREEN_KEY = "Fullscreen";
    private const string THEME_KEY = "Theme";
    private const string QUALITY_KEY = "Quality";
    private const string RESOLUTION_KEY = "Resolution";
    private const string RESOLUTION_WIDTH_KEY = "ResolutionWidth";
    private const string RESOLUTION_HEIGHT_KEY = "ResolutionHeight";

    public static float MusicVolume
    {
        get => Mathf.Clamp01(PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f));
        set
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(FULLSCREEN_KEY, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static int ThemeIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(THEME_KEY, 0), 0, 1);
        set
        {
            PlayerPrefs.SetInt(THEME_KEY, Mathf.Clamp(value, 0, 1));
            PlayerPrefs.Save();
        }
    }

    // UI order: High, Medium, Low.
    public static int QualityIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(QUALITY_KEY, 1), 0, 2);
        set
        {
            PlayerPrefs.SetInt(QUALITY_KEY, Mathf.Clamp(value, 0, 2));
            PlayerPrefs.Save();
        }
    }

    public static int ResolutionIndex
    {
        get => Mathf.Max(0, PlayerPrefs.GetInt(RESOLUTION_KEY, 0));
        set => PlayerPrefs.SetInt(RESOLUTION_KEY, Mathf.Max(0, value));
    }

    public static int ResolutionWidth =>
        PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY, Screen.currentResolution.width);

    public static int ResolutionHeight =>
        PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY, Screen.currentResolution.height);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateStartupApplier()
    {
        var settingsObject = new GameObject("GameSettingsStartupApplier");
        Object.DontDestroyOnLoad(settingsObject);
        settingsObject.AddComponent<GameSettingsStartupApplier>();
    }

    public static void SetResolution(int index, int width, int height)
    {
        ResolutionIndex = index;
        PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, Mathf.Max(1, width));
        PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, Mathf.Max(1, height));
        PlayerPrefs.Save();
    }

    public static void ApplySavedSettings()
    {
        // The menu exposes music volume, not a second master-volume slider.
        AudioListener.volume = 1f;
        ApplyQualitySettings();
    }

    public static void ApplyDisplaySettings()
    {
        ApplyFullscreenMode();
        ApplyResolution();
    }

    public static void ApplyFullscreenMode()
    {
        Screen.fullScreenMode = GetSavedScreenMode();
    }

    public static void ApplyResolution()
    {
        Screen.SetResolution(
            Mathf.Max(1, ResolutionWidth),
            Mathf.Max(1, ResolutionHeight),
            GetSavedScreenMode()
        );
    }

    private static FullScreenMode GetSavedScreenMode()
    {
        FullScreenMode screenMode = Fullscreen
            ? FullScreenMode.ExclusiveFullScreen
            : FullScreenMode.Windowed;

        return screenMode;
    }

    public static void ApplyQualitySettings()
    {
        int qualityCount = QualitySettings.names.Length;
        if (qualityCount == 0)
            return;

        int qualityLevel;
        switch (QualityIndex)
        {
            case 0:
                qualityLevel = qualityCount - 1;
                break;
            case 2:
                qualityLevel = 0;
                break;
            default:
                qualityLevel = Mathf.Clamp(qualityCount / 2, 0, qualityCount - 1);
                break;
        }

        QualitySettings.SetQualityLevel(qualityLevel, true);
    }
}

internal sealed class GameSettingsStartupApplier : MonoBehaviour
{
    private IEnumerator Start()
    {
        GameSettings.ApplySavedSettings();
        GameSettings.ApplyFullscreenMode();

        yield return null;

        GameSettings.ApplyResolution();
        Destroy(gameObject);
    }
}
