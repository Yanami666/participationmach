using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("跨场景状态")]
    public bool VisitedOffice = false;
    public bool VisitedParty = false;
    [Header("重启场景名")]
    public string startSceneName = "House";
    // 已播放的 NarrativeClip，key = AudioClip.name（无音频则用字幕前10字）
    public HashSet<string> PlayedClips = new HashSet<string>();
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
        PlayedClips.Clear();
        Instance = null;
        Destroy(gameObject);
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneName);
    }
    public void QuitGame() => Application.Quit();
    public static string GetClipKey(NarrativeClip clip)
    {
        if (clip.audioClip != null) return clip.audioClip.name;
        if (!string.IsNullOrEmpty(clip.subtitleText))
            return clip.subtitleText.Substring(0, Mathf.Min(10, clip.subtitleText.Length));
        return "";
    }
}