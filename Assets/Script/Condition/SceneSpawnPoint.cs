using UnityEngine;
using System.Collections;
public class SceneSpawnPoint : UnityEngine.MonoBehaviour
{
    public static string RequestedSpawnID = "";
    [Tooltip("和 TPAcrossScene 里的 spawnID 对应")]
    public string spawnID = "";
    void Start()
    {
        if (string.IsNullOrEmpty(RequestedSpawnID)) return;
        if (spawnID != RequestedSpawnID) return;
        StartCoroutine(SpawnNextFrame());
    }
    private IEnumerator SpawnNextFrame()
    {
        yield return null;
        UnityEngine.GameObject player = UnityEngine.GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            UnityEngine.Debug.LogWarning("[SceneSpawnPoint] 找不到Player");
            yield break;
        }
        var cc = player.GetComponent<UnityEngine.CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = transform.position;
        player.transform.rotation = transform.rotation;
        if (cc != null) cc.enabled = true;
        UnityEngine.Debug.Log($"[SceneSpawnPoint] 玩家落点：{gameObject.name}");
        RequestedSpawnID = "";
    }
}