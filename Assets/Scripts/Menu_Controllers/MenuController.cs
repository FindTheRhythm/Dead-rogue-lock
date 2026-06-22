using System.Collections;
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
    private Coroutine displayApplyRoutine;

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
        resolutions = GetUniqueResolutions();

        FillQualityDropdown();
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
        SaveAllSettings();
        GameSettings.ApplySavedSettings();

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
        musicVolumeSlider.SetValueWithoutNotify(GameSettings.MusicVolume);

        fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);

        themeDropdown.SetValueWithoutNotify(
            Mathf.Clamp(GameSettings.ThemeIndex, 0, themeDropdown.options.Count - 1)
        );

        qualityDropdown.SetValueWithoutNotify(
            Mathf.Clamp(GameSettings.QualityIndex, 0, qualityDropdown.options.Count - 1)
        );

        GameSettings.ApplySavedSettings();
    }

    public void ApplySettings()
    {
        SaveAllSettings();
        GameSettings.ApplySavedSettings();
        GameSettings.ApplyFullscreenMode();
        QueueDisplaySettingsApply();
    }

    public void OnVolumeChanged(float value)
    {
        GameSettings.MusicVolume = value;
    }

    public void OnFullscreenChanged(bool value)
    {
        GameSettings.Fullscreen = value;
        GameSettings.ApplyFullscreenMode();
        QueueDisplaySettingsApply();
    }

    public void OnThemeChanged(int value)
    {
        GameSettings.ThemeIndex = value;
    }

    public void OnQualityChanged(int value)
    {
        GameSettings.QualityIndex = value;
        GameSettings.ApplyQualitySettings();
    }

    public void OnResolutionChanged(int value)
    {
        if (resolutions == null || resolutions.Length == 0)
            return;

        value = Mathf.Clamp(value, 0, resolutions.Length - 1);

        Resolution resolution =
            resolutions[value];

        GameSettings.SetResolution(
            value,
            resolution.width,
            resolution.height
        );
        GameSettings.ApplyResolution();
    }

    private void QueueDisplaySettingsApply()
    {
        if (displayApplyRoutine != null)
            StopCoroutine(displayApplyRoutine);

        displayApplyRoutine = StartCoroutine(ApplyDisplaySettingsRoutine());
    }

    private IEnumerator ApplyDisplaySettingsRoutine()
    {
        yield return null;

        GameSettings.ApplyResolution();
        displayApplyRoutine = null;
    }

    private void FillResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        var options =
            new System.Collections.Generic.List<string>();

        int currentIndex = -1;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option =
                resolutions[i].width +
                " x " +
                resolutions[i].height;

            options.Add(option);

            if (resolutions[i].width == GameSettings.ResolutionWidth &&
                resolutions[i].height == GameSettings.ResolutionHeight)
            {
                currentIndex = i;
            }
        }

        if (currentIndex < 0)
        {
            currentIndex = Mathf.Clamp(
                GameSettings.ResolutionIndex,
                0,
                Mathf.Max(0, resolutions.Length - 1)
            );
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
        resolutionDropdown.RefreshShownValue();
    }

    private void FillQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(
            new System.Collections.Generic.List<string>
            {
                "High",
                "Medium",
                "Low"
            }
        );
    }

    private static Resolution[] GetUniqueResolutions()
    {
        Resolution[] availableResolutions = Screen.resolutions;
        var uniqueResolutions = new System.Collections.Generic.List<Resolution>();

        foreach (Resolution resolution in availableResolutions)
        {
            bool alreadyAdded = uniqueResolutions.Exists(
                item => item.width == resolution.width && item.height == resolution.height
            );

            if (!alreadyAdded)
                uniqueResolutions.Add(resolution);
        }

        if (uniqueResolutions.Count == 0)
            uniqueResolutions.Add(Screen.currentResolution);

        return uniqueResolutions.ToArray();
    }

    private void SaveAllSettings()
    {
        GameSettings.MusicVolume = musicVolumeSlider.value;

        GameSettings.Fullscreen = fullscreenToggle.isOn;
        GameSettings.ThemeIndex = themeDropdown.value;
        GameSettings.QualityIndex = qualityDropdown.value;

        if (resolutions != null && resolutions.Length > 0)
        {
            int resolutionIndex = Mathf.Clamp(
                resolutionDropdown.value,
                0,
                resolutions.Length - 1
            );
            Resolution resolution = resolutions[resolutionIndex];
            GameSettings.SetResolution(
                resolutionIndex,
                resolution.width,
                resolution.height
            );
        }

        PlayerPrefs.Save();
    }
}
