using UnityEngine;

// 两种模式合一：Zone（Collider进入）或 Raycast（被玩家看到）
public class NarrativeTrigger : UnityEngine.MonoBehaviour
{
    public enum TriggerType { Zone, Raycast }

    [Tooltip("Zone = 玩家走进触发；Raycast = 玩家看到触发")]
    public TriggerType triggerType = TriggerType.Zone;

    public NarrativeClip clip;

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (triggerType != TriggerType.Zone) return;
        if (!other.CompareTag("Player")) return;
        NarrativeManager.Instance?.TryPlay(clip);
    }
}