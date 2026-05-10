using UnityEngine;
using UnityEngine.SceneManagement;
public class TPAcrossScene : UnityEngine.MonoBehaviour
{
    [Header("目标场景")]
    public string targetSceneName = "";
    [Header("黑屏设置")]
    public float holdSeconds = 0.2f;
    [Header("可重复触发")]
    public bool triggerOnce = true;
    private bool _triggered = false;
    void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && _triggered) return;
        if (string.IsNullOrEmpty(targetSceneName))
        {
            UnityEngine.Debug.LogWarning("[TPAcrossScene] 未设置目标场景名！");
            return;
        }
        _triggered = true;
        ScreenFader.Instance?.FadeOutIn(() =>
        {
            SceneManager.LoadScene(targetSceneName);
        }, holdSeconds);
    }
}