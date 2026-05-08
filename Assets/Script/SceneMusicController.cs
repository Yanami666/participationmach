using System;
using System.Collections;
using UnityEngine;

public class SceneMusicController : MonoBehaviour
{
    [Header("音乐设置")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float targetVolume = 1f;
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 1.5f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        audioSource.Play();
        StartCoroutine(Fade(0f, targetVolume, fadeInDuration));
    }

    void OnDestroy()
    {
        // 场景卸载时淡出（仅在非应用退出时）
        // OnDestroy 里无法用 Coroutine，改用静态方法触发
    }

    // 供外部（如加载新场景前）调用的淡出接口
    public void FadeOutAndStop(System.Action onComplete = null)
    {
        StartCoroutine(FadeOutRoutine(onComplete));
    }

    private IEnumerator FadeOutRoutine(System.Action onComplete)
    {
        yield return StartCoroutine(Fade(audioSource.volume, 0f, fadeOutDuration));
        audioSource.Stop();
        onComplete?.Invoke();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        audioSource.volume = to;
    }
}