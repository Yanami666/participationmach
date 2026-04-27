using UnityEngine;

// 挂在目标场景的出生点 GameObject 上
// 场景加载后自动将玩家移动到此位置
public class PlayerSpawnPoint : UnityEngine.MonoBehaviour
{
    // 跨场景静态传递，不需要DontDestroyOnLoad
    public static string RequestedSpawnTag = "";

    void Start()
    {
        if (string.IsNullOrEmpty(RequestedSpawnTag)) return;
        if (!this.gameObject.CompareTag(RequestedSpawnTag)) return;

        UnityEngine.GameObject player = UnityEngine.GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            UnityEngine.Debug.LogWarning("[PlayerSpawnPoint] 找不到Player对象");
            return;
        }

        var cc = player.GetComponent<UnityEngine.CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = transform.position;
        player.transform.rotation = transform.rotation;

        if (cc != null) cc.enabled = true;

        UnityEngine.Debug.Log($"[PlayerSpawnPoint] 玩家已传送到 {gameObject.name}");
        RequestedSpawnTag = ""; // 清除，防止影响下次加载
    }
}