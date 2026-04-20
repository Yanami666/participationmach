using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    public static event System.Action OnGamePaused;
    public static event System.Action OnGameResumed;
    public static event System.Action OnGameOver;

    [Header("Pause UI / 暂停界面")]
    [SerializeField] private GameObject pauseMenuPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => SetGameState(GameState.Playing);

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            ReloadCurrentScene();
    }

    private void SetGameState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                OnGameOver?.Invoke();
                break;
        }
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            OnGamePaused?.Invoke();
        }
        else if (CurrentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            OnGameResumed?.Invoke();
        }
    }

    public void TriggerGameOver() => SetGameState(GameState.GameOver);
    public bool IsPlaying => CurrentState == GameState.Playing;
    public bool IsPaused => CurrentState == GameState.Paused;

    public void LoadScene(string sceneName) { Time.timeScale = 1f; SceneManager.LoadScene(sceneName); }
    public void LoadScene(int sceneIndex) { Time.timeScale = 1f; SceneManager.LoadScene(sceneIndex); }

    public void ReloadCurrentScene()
    {
        Instance = null;
        Destroy(gameObject);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame() => Application.Quit();
}