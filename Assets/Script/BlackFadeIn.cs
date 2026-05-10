using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlackFadeIn : MonoBehaviour
{
    [Header("µ≠»Î…Ë÷√")]
    public Image fadePanel;
    public float fadeDuration = 1f;

    void Start()
    {
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c = Color.black;
            c.a = 1f;
            fadePanel.color = c;
        }
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (timer / fadeDuration));
            if (fadePanel != null)
            {
                Color c = fadePanel.color;
                c.a = alpha;
                fadePanel.color = c;
            }
            yield return null;
        }
    }
}