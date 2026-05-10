using UnityEngine;

public class AniTrigger : UnityEngine.MonoBehaviour
{
    public enum ActionType { Inactive, Active, PlayAnimation }

    [Header("目标行为")]
    public ActionType actionType = ActionType.PlayAnimation;

    [Tooltip("执行行为的目标物体（留空则用自身）")]
    public UnityEngine.GameObject targetObject;

    [Tooltip("PlayAnimation 模式下，Animator 的 Trigger 参数名")]
    public string animatorTriggerName = "Play";

    [Tooltip("若使用旧版 Animation 组件，填写 clip 名称；留空则用 Animator")]
    public string legacyAnimationClipName = "";

    [Header("条件")]
    // 由 AniConditionTracker 自动注册，无需手动填写
    private int _totalConditions = 0;
    private int _completedConditions = 0;

    [Header("Debug")]
    public bool showGizmo = true;

    // 由 AniConditionTracker.Start() 调用
    public void RegisterCondition()
    {
        _totalConditions++;
        UnityEngine.Debug.Log($"[AniTrigger] 条件注册，当前共 {_totalConditions} 个");
    }

    // 由 AniConditionTracker 调用
    public void CompleteCondition()
    {
        _completedConditions++;
        UnityEngine.Debug.Log($"[AniTrigger] 条件完成 {_completedConditions}/{_totalConditions}");

        if (ConditionsMet())
        {
            ExecuteAction();
        }
    }

    private bool ConditionsMet()
    {
        if (_totalConditions == 0) return true;
        return _completedConditions >= _totalConditions;
    }

    private void ExecuteAction()
    {
        UnityEngine.GameObject target = targetObject != null ? targetObject : gameObject;

        if (actionType == ActionType.Inactive)
        {
            target.SetActive(false);
            UnityEngine.Debug.Log($"[AniTrigger] {target.name} 已 SetActive(false)");
        }
        else if (actionType == ActionType.Active)
        {
            target.SetActive(true);
            UnityEngine.Debug.Log($"[AniTrigger] {target.name} 已 SetActive(true)");
        }
        else if (actionType == ActionType.PlayAnimation)
        {
            if (string.IsNullOrEmpty(legacyAnimationClipName))
            {
                var animator = target.GetComponent<UnityEngine.Animator>();
                if (animator != null)
                {
                    animator.SetTrigger(animatorTriggerName);
                    UnityEngine.Debug.Log($"[AniTrigger] Animator Trigger '{animatorTriggerName}' 已触发");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[AniTrigger] {target.name} 没有 Animator 组件");
                }
            }
            else
            {
                var anim = target.GetComponent<UnityEngine.Animation>();
                if (anim != null)
                {
                    anim.Play(legacyAnimationClipName);
                    UnityEngine.Debug.Log($"[AniTrigger] Animation clip '{legacyAnimationClipName}' 已播放");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[AniTrigger] {target.name} 没有 Animation 组件");
                }
            }
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