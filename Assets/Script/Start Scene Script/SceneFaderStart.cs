// SceneFaderStart.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class SceneFaderStart : MonoBehaviour
{
    [Header("Fader")]
    public Image blackOverlay;
    public float fadeOutDuration = 0.5f;   // 开场渐出（黑→透明）
    public float fadeInDuration = 1.0f;    // 结束渐入（透明→黑）

    void Start()
    {
        // 开场：从全黑渐出
        blackOverlay.color = new Color(0, 0, 0, 1);
        StartCoroutine(FadeRoutine(1f, 0f, fadeOutDuration, null));
    }

    public void FadeToBlack(Action onComplete)
    {
        StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration, onComplete));
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