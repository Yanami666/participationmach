using UnityEngine;
using System.Collections;
using System;

public class DirtSpawner : UnityEngine.MonoBehaviour
{
    [Header("引用")]
    public UnityEngine.GameObject dirtDecalPrefab;
    public UnityEngine.LayerMask groundLayer;

    [Header("参数")]
    public float spawnInterval = 1.5f;
    public int maxDirtCount = 20;
    public UnityEngine.Vector2 spawnAreaSize = new UnityEngine.Vector2(5f, 5f);
    public float groundCheckDistance = 10f;

    private int _currentDirtCount = 0;
    private bool _isSpawning = false;
    private Coroutine _spawnCoroutine;

    public void StartSpawning()
    {
        if (_isSpawning) return;
        _isSpawning = true;
        _spawnCoroutine = StartCoroutine(SpawnLoop());
        UnityEngine.Debug.Log("[DirtSpawner] 开始生成 Dirt");
    }

    public void StopSpawning()
    {
        if (!_isSpawning) return;
        _isSpawning = false;
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
        UnityEngine.Debug.Log("[DirtSpawner] 停止生成 Dirt");
    }

    // 由 DirtDecal 的 SetActive(false) → OnDisable 通知计数减少
    public void OnDirtCleaned()
    {
        _currentDirtCount = Mathf.Max(0, _currentDirtCount - 1);
    }

    private IEnumerator SpawnLoop()
    {
        while (_isSpawning)
        {
            if (_currentDirtCount < maxDirtCount)
                TrySpawnDirt();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void TrySpawnDirt()
    {
        // 在 XZ 平面随机取点
        float x = UnityEngine.Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f);
        float z = UnityEngine.Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f);
        UnityEngine.Vector3 origin = transform.position + new UnityEngine.Vector3(x, groundCheckDistance * 0.5f, z);

        if (UnityEngine.Physics.Raycast(origin, UnityEngine.Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            UnityEngine.GameObject dirt = Instantiate(dirtDecalPrefab, hit.point,
                UnityEngine.Quaternion.FromToRotation(UnityEngine.Vector3.up, hit.normal));

            // 自动挂 AniConditionTracker 不需要，DirtDecal.SetActive(false) 触发 OnDisable 即可
            // 但如果 Prefab 上有 AniConditionTracker 需要手动拖，这里不做自动注入

            _currentDirtCount++;
            UnityEngine.Debug.Log($"[DirtSpawner] 生成 Dirt，当前数量 {_currentDirtCount}/{maxDirtCount}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        UnityEngine.Gizmos.color = new UnityEngine.Color(0f, 0.5f, 1f, 0.3f);
        UnityEngine.Gizmos.DrawCube(transform.position,
            new UnityEngine.Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));
        UnityEngine.Gizmos.color = new UnityEngine.Color(0f, 0.5f, 1f, 0.8f);
        UnityEngine.Gizmos.DrawWireCube(transform.position,
            new UnityEngine.Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));
    }
}