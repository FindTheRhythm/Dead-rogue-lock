using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class InGameMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string levelSceneName = "Level_1";

    private GameInput input;

    private bool isPaused;
    private bool isDead;
    private bool isWon;

    private PlayerHealth playerHealth;
    private BossAnimator boss;

    private bool subscribed;
    private bool bossSubscribed;

    private void Awake()
    {
        input = new GameInput();
    }

    private void Start()
    {
        HideAll();
        GamePauseState.Resume();
    }

    private void Update()
    {
        if (subscribed)
            return;

        TryFindPlayer();

        if (!bossSubscribed)
        {
            TryFindBoss();
        }
    }

    private void TryFindPlayer()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerHealth.OnPlayerDied += ShowDeath;
        subscribed = true;

        Debug.Log("[InGameMenuController] PlayerHealth subscribed!");
    }

    private void TryFindBoss()
    {
        boss = FindFirstObjectByType<BossAnimator>();

        if (boss == null)
            return;

        boss.OnBossDied += ShowWin;
        bossSubscribed = true;

        Debug.Log("[InGameMenuController] Boss subscribed!");
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Pause.performed += OnPause;
    }

    private void OnDisable()
    {
        input.Player.Pause.performed -= OnPause;
        input.Disable();

        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied -= ShowDeath;
        }

        if (boss != null)
            boss.OnBossDied -= ShowWin;
    }

    // =========================
    // INPUT
    // =========================
    private void OnPause(InputAction.CallbackContext ctx)
    {
        if (isDead || isWon)
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // =========================
    // PAUSE
    // =========================
    public void PauseGame()
    {
        if (isDead || isWon)
            return;

        isPaused = true;

        HideAll();
        pausePanel.SetActive(true);

        GamePauseState.Pause();
    }

    public void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);

        GamePauseState.Resume();
    }

    // =========================
    // DEATH
    // =========================
    public void ShowDeath()
    {
        MusicManager.Instance.PlayDeath();
        isDead = true;
        isPaused = false;

        HideAll();
        deathPanel.SetActive(true);

        GamePauseState.Pause(false);
    }

    // =========================
    // WIN
    // =========================
    public void ShowWin()
    {
        MusicManager.Instance.PlayVictory();
        isWon = true;
        isPaused = false;

        HideAll();
        winPanel.SetActive(true);

        GamePauseState.Pause(false);
    }

    // =========================
    // BUTTONS
    // =========================
    public void RestartLevel()
    {
        GamePauseState.Resume();
        MusicManager.Instance.PlayExploration();
        SceneManager.LoadScene(levelSceneName);
    }

    public void ReturnToMenu()
    {
        GamePauseState.Resume();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // =========================
    // UTILS
    // =========================
    private void HideAll()
    {
        pausePanel.SetActive(false);
        deathPanel.SetActive(false);
        winPanel.SetActive(false);
    }
}
