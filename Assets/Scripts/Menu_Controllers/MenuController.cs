using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject exitGamePanel;

    [Header("Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [SerializeField] private TMP_Dropdown themeDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    private GameInput input;

    private void Awake()
    {
        input = new GameInput();
    }

    private void OnEnable()
    {
        input.Enable();

        input.UI.Cancel.performed += OnBack;
    }

    private void OnDisable()
    {
        input.UI.Cancel.performed -= OnBack;

        input.Disable();
    }

    private void Start()
    {
        resolutions =
            Screen.resolutions;

        FillResolutionDropdown();

        LoadSettings();

        ShowMainPanel();
    }

    private void OnBack(
        InputAction.CallbackContext context)
    {
        HandleEscape();
    }

    private void HandleEscape()
    {
        if (settingsPanel.activeSelf)
        {
            ShowMainPanel();
            return;
        }

        if (exitGamePanel.activeSelf)
        {
            ShowMainPanel();
            return;
        }

        if (mainPanel.activeSelf)
        {
            ShowExitPanel();
        }
    }

    // =====================
    // PANELS
    // =====================

    public void ShowMainPanel()
    {
        SaveAllSettings();
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        exitGamePanel.SetActive(false);
    }

    public void ShowSettingsPanel()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
        exitGamePanel.SetActive(false);
    }

    public void ShowExitPanel()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        exitGamePanel.SetActive(true);
    }

    // =====================
    // GAME
    // =====================

    public void StartGame()
    {
        GamePauseState.Resume();

        SceneManager.LoadScene("Level_1");
    }

    public void ExitGame()
    {
        SaveAllSettings();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =====================
    // SETTINGS
    // =====================

    private void LoadSettings()
    {
        musicVolumeSlider.value =
            GameSettings.MusicVolume;

        fullscreenToggle.isOn =
            GameSettings.Fullscreen;

        themeDropdown.value =
            GameSettings.ThemeIndex;

        qualityDropdown.value =
            GameSettings.QualityIndex;

        resolutionDropdown.value =
            GameSettings.ResolutionIndex;

        ApplySettings();
    }

    public void ApplySettings()
    {
        AudioListener.volume =
            musicVolumeSlider.value;

        Screen.fullScreen =
            fullscreenToggle.isOn;

        QualitySettings.SetQualityLevel(
            qualityDropdown.value
        );

        Resolution resolution =
            resolutions[
                resolutionDropdown.value
            ];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            fullscreenToggle.isOn
        );
    }

    public void OnVolumeChanged(float value)
    {
        GameSettings.MusicVolume = value;
        GameSettings.MasterVolume = value;
    }

    public void OnFullscreenChanged(bool value)
    {
        GameSettings.Fullscreen = value;

        Screen.fullScreen = value;
    }

    public void OnThemeChanged(int value)
    {
        GameSettings.ThemeIndex = value;
    }

    public void OnQualityChanged(int value)
    {
        GameSettings.QualityIndex = value;

        QualitySettings.SetQualityLevel(value);
    }

    public void OnResolutionChanged(int value)
    {
        GameSettings.ResolutionIndex = value;

        Resolution resolution =
            resolutions[value];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            fullscreenToggle.isOn
        );
    }

    private void FillResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        var options =
            new System.Collections.Generic.List<string>();

        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option =
                resolutions[i].width +
                " x " +
                resolutions[i].height;

            options.Add(option);

            if (resolutions[i].width ==
                Screen.currentResolution.width &&
                resolutions[i].height ==
                Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        resolutionDropdown.value =
            currentIndex;

        resolutionDropdown.RefreshShownValue();
    }

    private void SaveAllSettings()
    {
        GameSettings.MusicVolume = musicVolumeSlider.value;
        GameSettings.MasterVolume = musicVolumeSlider.value;

        GameSettings.Fullscreen = fullscreenToggle.isOn;
        GameSettings.ThemeIndex = themeDropdown.value;
        GameSettings.QualityIndex = qualityDropdown.value;
        GameSettings.ResolutionIndex = resolutionDropdown.value;

        PlayerPrefs.Save();
    }
}