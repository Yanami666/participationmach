using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitDoor : MonoBehaviour
{
    [Header("场景设置")]
    public string nextSceneName;

    [Header("触发设置")]
    public float triggerRadius = 3f;
    public Transform player;

    [Header("淡出设置")]
    public Image fadePanel;
    public float fadeDuration = 1f;

    private bool triggered = false;

    void Start()
    {
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
        }
    }

    void Update()
    {
        if (triggered) return;
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= triggerRadius)
        {
            triggered = true;
            StartCoroutine(FadeAndLoad());
        }
    }

    IEnumerator FadeAndLoad()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            if (fadePanel != null)
            {
                Color c = fadePanel.color;
                c.a = alpha;
                fadePanel.color = c;
            }
            yield return null;
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("ExitDoor：没有设置 nextSceneName！");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}