// TitleSceneManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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

    [Header("点Start时要隐藏的物体")]
    public List<GameObject> hideOnStart;

    [Header("Lamp")]
    public Rigidbody lampRigidbody;
    public AudioSource audioSource;
    public AudioClip dropSound;
    public AudioClip impactSound;
    public float impactVelocityThreshold = 2f;

    [Header("Scene")]
    public string firstGameScene = "House";
    public float waitBeforeFade = 1.8f;

    [Header("References")]
    public SceneFaderStart fader;
    public LampFlicker lampFlicker;

    private bool hasPlayedImpact = false;

    void Start()
    {
        settingsPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    public void OnStartClicked()
    {
        if (currentState != TitleState.Main) return;
        currentState = TitleState.Transitioning;

        foreach (var obj in hideOnStart)
            if (obj != null) obj.SetActive(false);

        if (lampFlicker != null)
            lampFlicker.StopFlicker();

        if (audioSource != null && dropSound != null)
            audioSource.PlayOneShot(dropSound);

        if (lampRigidbody != null)
        {
            lampRigidbody.isKinematic = false;
            lampRigidbody.useGravity = true;
        }

        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        yield return new WaitForSeconds(waitBeforeFade);
        fader.FadeToBlack(() =>
        {
            SceneManager.LoadScene(firstGameScene);
        });
    }

    public void OnSettingsClicked()
    {
        if (currentState != TitleState.Main) return;
        currentState = TitleState.Settings;

        mainButtonsPanel.SetActive(false);
        settingsPanel.SetActive(true);
        backButton.gameObject.SetActive(true);
    }

    public void OnBackClicked()
    {
        if (currentState != TitleState.Settings) return;
        currentState = TitleState.Main;

        settingsPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
        mainButtonsPanel.SetActive(true);
    }
}