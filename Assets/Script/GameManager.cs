using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("跨场景状态")]
    public bool VisitedOffice = false;
    public bool VisitedParty = false;
    [Header("重启场景名")]
    public string startSceneName = "House";
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            ReloadToStart();
    }
    public void ReloadToStart()
    {
        VisitedOffice = false;
        VisitedParty = false;
        Instance = null;
        Destroy(gameObject);
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneName);
    }
    public void QuitGame() => Application.Quit();
}