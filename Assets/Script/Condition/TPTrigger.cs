using UnityEngine;
using UnityEngine.SceneManagement;

public class TPTrigger : UnityEngine.MonoBehaviour
{
    [Header("目标设置")]
    [Tooltip("留空 = 同场景TP；填写场景名 = 切换场景")]
    public string targetSceneName = "";

    [Tooltip("同场景TP时的目标位置（留空则用targetSceneName）")]
    public UnityEngine.Transform targetPoint;

    [Tooltip("切换场景时，目标场景里 PlayerSpawnPoint 的标签（默认 SpawnPoint）")]
    public string spawnPointTag = "SpawnPoint";

    [Header("条件")]
    // 由 TPConditionTracker 自动注册，无需手动填写
    private int _totalConditions = 0;
    private int _completedConditions = 0;

    [Header("Debug")]
    public bool showGizmo = true;

    // 由 TPConditionTracker.Start() 调用
    public void RegisterCondition()
    {
        _totalConditions++;
        UnityEngine.Debug.Log($"[TPTrigger] 条件注册，当前共 {_totalConditions} 个");
    }

    // 由 TPConditionTracker.OnDisable/OnDestroy 调用
    public void CompleteCondition()
    {
        _completedConditions++;
        UnityEngine.Debug.Log($"[TPTrigger] 条件完成 {_completedConditions}/{_totalConditions}");
    }

    private bool ConditionsMet()
    {
        // 没有注册任何条件时直接通过（无条件TP）
        if (_totalConditions == 0) return true;
        return _completedConditions >= _totalConditions;
    }

    void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!ConditionsMet())
        {
            UnityEngine.Debug.Log("[TPTrigger] 条件未满足，无法传送");
            return;
        }

        if (!string.IsNullOrEmpty(targetSceneName))
        {
            // 切换场景
            PlayerSpawnPoint.RequestedSpawnTag = spawnPointTag;
            SceneManager.LoadScene(targetSceneName);
        }
        else if (targetPoint != null)
        {
            // 同场景TP
            UnityEngine.GameObject player = other.gameObject;
            // 禁用CharacterController/Rigidbody防止位移抵抗
            var cc = player.GetComponent<UnityEngine.CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = targetPoint.position;
            player.transform.rotation = targetPoint.rotation;

            if (cc != null) cc.enabled = true;
        }
        else
        {
            UnityEngine.Debug.LogWarning("[TPTrigger] 未设置目标点或目标场景！");
        }
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