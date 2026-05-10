using UnityEngine;
public class TPDoorConditionTracker : UnityEngine.MonoBehaviour
{
    public enum ConditionType { Moved, Disabled }
    [Tooltip("注册到哪个 TPDoor")]
    public TPDoor targetTrigger;
    public ConditionType conditionType = ConditionType.Moved;
    [Tooltip("Moved 模式下位移超过多少算完成")]
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
        if (dist >= moveThreshold) Complete();
    }
    void OnDisable()
    {
        if (_completed || !_registered) return;
        if (conditionType == ConditionType.Disabled) Complete();
    }
    void OnDestroy()
    {
        if (_completed || !_registered) return;
        if (conditionType == ConditionType.Disabled) Complete();
    }
    private void Complete()
    {
        _completed = true;
        targetTrigger?.CompleteCondition();
        UnityEngine.Debug.Log($"[TPDoorConditionTracker] {gameObject.name} 条件完成");
    }
}