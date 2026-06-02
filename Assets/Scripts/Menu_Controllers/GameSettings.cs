using UnityEngine;

public static class GameSettings
{
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string FULLSCREEN_KEY = "Fullscreen";
    private const string THEME_KEY = "Theme";
    private const string QUALITY_KEY = "Quality";
    private const string RESOLUTION_KEY = "Resolution";

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        set
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
            PlayerPrefs.Save();
        }
    }

    private const string MASTER_VOLUME_KEY = "MasterVolume";

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        set
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
            PlayerPrefs.Save();
            AudioListener.volume = value;
        }
    }
    
    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(
                FULLSCREEN_KEY,
                value ? 1 : 0
            );

            PlayerPrefs.Save();
        }
    }

    public static int ThemeIndex
    {
        get => PlayerPrefs.GetInt(THEME_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(THEME_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public static int QualityIndex
    {
        get => PlayerPrefs.GetInt(QUALITY_KEY, 2);
        set
        {
            PlayerPrefs.SetInt(QUALITY_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public static int ResolutionIndex
    {
        get => PlayerPrefs.GetInt(RESOLUTION_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(RESOLUTION_KEY, value);
            PlayerPrefs.Save();
        }
    }
}