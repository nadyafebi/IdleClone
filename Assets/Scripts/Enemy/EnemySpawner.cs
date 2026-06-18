using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    #region Serialized Fields

    [Header("Spawn")]
    [SerializeField]
    private EnemyData _enemyData;

    [SerializeField]
    private GameObject _enemyPrefab;

    [SerializeField]
    [Tooltip("Maximum number of enemies alive at once from this spawner.")]
    private int _maxCount = 3;

    [Header("Debug")]
    [SerializeField]
    private bool _drawGizmos = true;

    #endregion

    #region Private Fields

    private PlatformGraphBuilder _graphBuilder;
    private List<PlatformNode> _platformNodes = new();
    private List<GameObject> _pool = new();

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

        PlatformNode anchor = _graphBuilder.FindNearestNode(transform.position, groundOnly: true);
        if (anchor == null)
        {
            Debug.LogError("[EnemySpawner] No ground node near spawner position!");
            enabled = false;
            return;
        }

        CollectPlatformNodes(anchor);
        BuildPool();
        SpawnAll();
    }

    #endregion

    #region Pool

    private void BuildPool()
    {
        for (int i = 0; i < _maxCount; i++)
        {
            GameObject go = Instantiate(_enemyPrefab, transform);
            go.SetActive(false);

            EnemyHealth health = go.GetComponent<EnemyHealth>();
            if (health == null)
            {
                Debug.LogError("[EnemySpawner] Enemy prefab is missing EnemyHealth!");
                Destroy(go);
                continue;
            }

            health.OnDied += () => HandleEnemyDied(go);
            _pool.Add(go);
        }
    }

    private void SpawnAll()
    {
        // Distribute enemies across platform nodes without stacking on initial spawn.
        var available = new List<PlatformNode>(_platformNodes);
        foreach (GameObject go in _pool)
        {
            if (available.Count == 0)
                available.AddRange(_platformNodes);

            int idx = Random.Range(0, available.Count);
            SpawnEnemy(go, available[idx]);
            available.RemoveAt(idx);
        }
    }

    private void SpawnEnemy(GameObject go, PlatformNode node)
    {
        go.GetComponent<Enemy>().SetData(_enemyData);
        go.GetComponent<EnemyHealth>().ResetHealth();
        go.SetActive(true);
        go.GetComponent<EnemyMovement>().Activate(node);
    }

    private void HandleEnemyDied(GameObject go)
    {
        go.SetActive(false);
        StartCoroutine(RespawnAfterCooldown(go));
    }

    private IEnumerator RespawnAfterCooldown(GameObject go)
    {
        yield return new WaitForSeconds(_enemyData.RespawnCooldown);
        PlatformNode node = _platformNodes[Random.Range(0, _platformNodes.Count)];
        SpawnEnemy(go, node);
    }

    #endregion

    #region Platform Detection

    private void CollectPlatformNodes(PlatformNode anchor)
    {
        _platformNodes.Clear();
        var visited = new HashSet<PlatformNode>();
        var queue = new Queue<PlatformNode>();

        queue.Enqueue(anchor);
        visited.Add(anchor);

        while (queue.Count > 0)
        {
            PlatformNode node = queue.Dequeue();
            _platformNodes.Add(node);

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
    }

    #endregion

    #region Editor Visualisation

    private void OnDrawGizmos()
    {
        if (!_drawGizmos || _platformNodes == null || _platformNodes.Count == 0)
            return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
        foreach (var node in _platformNodes)
            Gizmos.DrawSphere(node.worldPosition, 0.18f);
    }

    #endregion
}
