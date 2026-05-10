// SceneFaderStart.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class SceneFaderStart : MonoBehaviour
{
    [Header("Fader")]
    public Image blackOverlay;
    public float fadeOutDuration = 0.5f;
    public float fadeInDuration = 1.0f;

    [Header("Fade完成后隐藏的物体根节点")]
    public GameObject[] rootObjectsToHide;

    void Start()
    {
        blackOverlay.color = new Color(0, 0, 0, 1);
        StartCoroutine(FadeRoutine(1f, 0f, fadeOutDuration, null));
    }

    public void FadeToBlack(Action onComplete)
    {
        StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration, () =>
        {
            HideEverything();
            onComplete?.Invoke();
        }));
    }

    void HideEverything()
    {
        // 隐藏Inspector里指定的根节点
        foreach (var obj in rootObjectsToHide)
            if (obj != null) obj.SetActive(false);
    }

    IEnumerator FadeRoutine(float from, float to, float duration, Action onComplete)
    {
        float elapsed = 0f;
        Color c = blackOverlay.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            blackOverlay.color = c;
            yield return null;
        }

        c.a = to;
        blackOverlay.color = c;
        onComplete?.Invoke();
    }
}