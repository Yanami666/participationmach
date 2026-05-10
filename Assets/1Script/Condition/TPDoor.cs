using UnityEngine;
using UnityEngine.SceneManagement;
public class TPDoor : UnityEngine.MonoBehaviour
{
    [Header("目标设置")]
    public string targetSceneName = "";
    [Tooltip("目标场景里 SceneSpawnPoint 的 spawnID，留空则不指定")]
    public string spawnID = "";
    [Tooltip("Office 或 Party，留空则不标记")]
    public string markVisited = "";
    [Header("黑屏设置")]
    public float holdSeconds = 0.2f;
    [Header("可重复触发")]
    public bool triggerOnce = false;
    private bool _triggered = false;
    [Header("条件")]
    private int _totalConditions = 0;
    private int _completedConditions = 0;
    [Header("Debug")]
    public bool showGizmo = true;
    public void RegisterCondition()
    {
        _totalConditions++;
        UnityEngine.Debug.Log($"[TPDoor] 条件注册，当前共 {_totalConditions} 个");
    }
    public void CompleteCondition()
    {
        _completedConditions++;
        UnityEngine.Debug.Log($"[TPDoor] 条件完成 {_completedConditions}/{_totalConditions}");
    }
    private bool ConditionsMet()
    {
        if (_totalConditions == 0) return true;
        return _completedConditions >= _totalConditions;
    }
    void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && _triggered) return;
        if (!ConditionsMet())
        {
            UnityEngine.Debug.Log("[TPDoor] 条件未满足，无法传送");
            return;
        }
        if (string.IsNullOrEmpty(targetSceneName))
        {
            UnityEngine.Debug.LogWarning("[TPDoor] 未设置目标场景名！");
            return;
        }
        _triggered = true;
        if (!string.IsNullOrEmpty(markVisited) && GameManager.Instance != null)
        {
            if (markVisited == "Office") GameManager.Instance.VisitedOffice = true;
            else if (markVisited == "Party") GameManager.Instance.VisitedParty = true;
        }
        SceneSpawnPoint.RequestedSpawnID = spawnID;
        ScreenFader.Instance?.FadeOutIn(() =>
        {
            SceneManager.LoadScene(targetSceneName);
        }, holdSeconds);
    }
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        bool met = ConditionsMet();
        UnityEngine.Gizmos.color = met ? UnityEngine.Color.green : UnityEngine.Color.red;
        var col = GetComponent<UnityEngine.Collider>();
        if (col != null)
            UnityEngine.Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}