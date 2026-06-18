using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnPoint
{
    #region Serialized Fields

    [SerializeField]
    private Transform _point;

    [SerializeField]
    [Min(1)]
    [Tooltip("Maximum number of enemies alive at once from this spawn point.")]
    private int _maxCount = 3;

    #endregion

    #region Public Properties

    public Transform Point => _point;
    public int MaxCount => _maxCount;

    #endregion
}

public class EnemySpawner : MonoBehaviour
{
    #region Serialized Fields

    [Header("Spawn")]
    [SerializeField]
    private EnemyData _enemyData;

    [SerializeField]
    private GameObject _enemyPrefab;

    [SerializeField]
    private List<SpawnPoint> _spawnPoints = new();

    [Header("Debug")]
    [SerializeField]
    private bool _drawGizmos = true;

    #endregion

    #region Private Types

    private readonly struct PoolEntry
    {
        public readonly Enemy Enemy;
        public readonly EnemyHealth Health;
        public readonly EnemyMovement Movement;
        public readonly PlatformNode[] PlatformNodes;

        public PoolEntry(Enemy enemy, EnemyHealth health, EnemyMovement movement, PlatformNode[] platformNodes)
        {
            Enemy = enemy;
            Health = health;
            Movement = movement;
            PlatformNodes = platformNodes;
        }
    }

    #endregion

    #region Private Fields

    private PlatformGraphBuilder _graphBuilder;
    private readonly List<PoolEntry> _pool = new();

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _graphBuilder = FindFirstObjectByType<PlatformGraphBuilder>();
        if (_graphBuilder == null)
        {
            Debug.LogError("[EnemySpawner] No PlatformGraphBuilder found in scene!");
            enabled = false;
            return;
        }

        if (_enemyData == null)
        {
            Debug.LogError("[EnemySpawner] No EnemyData assigned!");
            enabled = false;
            return;
        }

        if (_enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] No enemy prefab assigned!");
            enabled = false;
            return;
        }

        if (_spawnPoints.Count == 0)
        {
            Debug.LogError("[EnemySpawner] No spawn points configured!");
            enabled = false;
            return;
        }

        BuildPool();
        SpawnAll();
    }

    #endregion

    #region Pool

    private void BuildPool()
    {
        foreach (SpawnPoint sp in _spawnPoints)
        {
            if (sp.Point == null)
            {
                Debug.LogWarning("[EnemySpawner] Spawn point has no transform assigned, skipping.");
                continue;
            }

            PlatformNode anchor = _graphBuilder.FindNearestNode(sp.Point.position, groundOnly: true);
            if (anchor == null)
            {
                Debug.LogWarning($"[EnemySpawner] No ground node near '{sp.Point.name}', skipping.");
                continue;
            }

            PlatformNode[] platformNodes = CollectPlatformNodes(anchor);

            for (int i = 0; i < sp.MaxCount; i++)
            {
                GameObject go = Instantiate(_enemyPrefab, transform);
                go.SetActive(false);

                Enemy enemy = go.GetComponent<Enemy>();
                EnemyHealth health = go.GetComponent<EnemyHealth>();
                EnemyMovement movement = go.GetComponent<EnemyMovement>();

                if (enemy == null || health == null || movement == null)
                {
                    Debug.LogError("[EnemySpawner] Enemy prefab is missing a required component!");
                    Destroy(go);
                    continue;
                }

                var entry = new PoolEntry(enemy, health, movement, platformNodes);
                health.OnDied += () => HandleEnemyDied(entry);
                _pool.Add(entry);
            }
        }
    }

    private void SpawnAll()
    {
        foreach (PoolEntry entry in _pool)
            SpawnEnemy(entry);
    }

    private void SpawnEnemy(PoolEntry entry)
    {
        entry.Enemy.SetData(_enemyData);
        entry.Health.ResetHealth();
        entry.Enemy.gameObject.SetActive(true);
        PlatformNode node = entry.PlatformNodes[UnityEngine.Random.Range(0, entry.PlatformNodes.Length)];
        entry.Movement.Activate(node);
    }

    private PlatformNode[] CollectPlatformNodes(PlatformNode anchor)
    {
        var result = new List<PlatformNode>();
        var visited = new HashSet<PlatformNode>();
        var queue = new Queue<PlatformNode>();

        queue.Enqueue(anchor);
        visited.Add(anchor);

        while (queue.Count > 0)
        {
            PlatformNode node = queue.Dequeue();
            result.Add(node);

            foreach (PlatformNode neighbor in node.neighbors)
            {
                if (visited.Contains(neighbor))
                    continue;
                if (!Mathf.Approximately(neighbor.worldPosition.y, anchor.worldPosition.y))
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return result.ToArray();
    }

    private void HandleEnemyDied(PoolEntry entry)
    {
        entry.Enemy.gameObject.SetActive(false);
        StartCoroutine(RespawnAfterCooldown(entry));
    }

    private IEnumerator RespawnAfterCooldown(PoolEntry entry)
    {
        yield return new WaitForSeconds(_enemyData.RespawnCooldown);
        SpawnEnemy(entry);
    }

    #endregion

    #region Editor Visualisation

    private void OnDrawGizmos()
    {
        if (!_drawGizmos || _spawnPoints == null)
            return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
        foreach (SpawnPoint sp in _spawnPoints)
        {
            if (sp.Point != null)
                Gizmos.DrawSphere(sp.Point.position, 0.18f);
        }
    }

    #endregion
}
