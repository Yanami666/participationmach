using UnityEngine;

public class AniConditionTracker : UnityEngine.MonoBehaviour
{
    public enum ConditionType { Moved, Cleaned }

    [Tooltip("注册到哪个 AniTrigger")]
    public AniTrigger targetTrigger;

    [Tooltip("Moved = 位移超过阈值算完成；Cleaned = 外部调用 MarkCleaned() 或 SetActive(false) 算完成")]
    public ConditionType conditionType = ConditionType.Cleaned;

    [Tooltip("Moved 模式下，位移超过多少算完成")]
    public float moveThreshold = 0.5f;

    private UnityEngine.Vector3 _startPosition;
    private bool _registered = false;
    private bool _completed = false;

    void Start()
    {
        _startPosition = transform.position;

        if (targetTrigger != null)
        {
            targetTrigger.RegisterCondition();
            _registered = true;
        }
    }

    void Update()
    {
        if (_completed || !_registered) return;
        if (conditionType != ConditionType.Moved) return;

        float dist = UnityEngine.Vector3.Distance(transform.position, _startPosition);
        if (dist >= moveThreshold)
        {
            Complete();
        }
    }

    // Cleaned 模式：由外部清理脚本主动调用
    public void MarkCleaned()
    {
        if (_completed || !_registered) return;
        if (conditionType == ConditionType.Cleaned)
        {
            Complete();
        }
    }

    void OnDisable()
    {
        if (_completed || !_registered) return;
        if (conditionType == ConditionType.Cleaned)
        {
            Complete();
        }
    }

    void OnDestroy()
    {
        if (_completed || !_registered) return;
        if (conditionType == ConditionType.Cleaned)
        {
            Complete();
        }
    }

    private void Complete()
    {
        _completed = true;
        targetTrigger?.CompleteCondition();
        UnityEngine.Debug.Log($"[AniConditionTracker] {gameObject.name} 条件完成");
    }
}