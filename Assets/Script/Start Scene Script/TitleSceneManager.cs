// TitleSceneManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    public enum TitleState { Main, Settings, Transitioning }
    public TitleState currentState = TitleState.Main;

    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button backButton;

    [Header("Panels")]
    public GameObject mainButtonsPanel;
    public GameObject settingsPanel;

    [Header("References")]
    public TitleCameraController cameraController;
    public LampDropController lampDrop;
    public SceneFaderStart fader;

    [Header("Scene")]
    public string firstGameScene = "House";

    void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        backButton.onClick.AddListener(OnBackClicked);

        settingsPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    void OnStartClicked()
    {
        if (currentState != TitleState.Main) return;
        currentState = TitleState.Transitioning;

        mainButtonsPanel.SetActive(false);
        lampDrop.DropLamp();
        StartCoroutine(StartSequence());
    }

    System.Collections.IEnumerator StartSequence()
    {
        // 等灯落地（可调）
        yield return new WaitForSeconds(1.8f);
        fader.FadeToBlack(() =>
        {
            SceneManager.LoadScene(firstGameScene);
        });
    }

    void OnSettingsClicked()
    {
        if (currentState != TitleState.Main) return;
        currentState = TitleState.Settings;

        mainButtonsPanel.SetActive(false);
        settingsPanel.SetActive(true);
        backButton.gameObject.SetActive(true);
        cameraController.MoveToSettingsPosition();
    }

    void OnBackClicked()
    {
        if (currentState != TitleState.Settings) return;
        currentState = TitleState.Main;

        settingsPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
        mainButtonsPanel.SetActive(true);
        cameraController.MoveToMainPosition();
    }
}