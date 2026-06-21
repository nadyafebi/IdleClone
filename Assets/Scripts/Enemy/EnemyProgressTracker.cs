using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProgressTracker : MonoBehaviour
{
    #region Public Properties

    public event Action<EnemyData> OnKillCountUpdated;

    public IReadOnlyDictionary<EnemyData, int> KillCounts => _killCounts;

    #endregion

    #region Private Fields

    private readonly Dictionary<EnemyData, int> _killCounts = new();

    #endregion

    #region Unity Lifecycle

    private void OnEnable() => Enemy.OnAnyEnemyKilled += HandleEnemyKilled;

    private void OnDisable() => Enemy.OnAnyEnemyKilled -= HandleEnemyKilled;

    #endregion

    #region Public Methods

    public int GetKillCount(EnemyData enemy)
    {
        if (enemy == null)
            return 0;
        _killCounts.TryGetValue(enemy, out int count);
        return count;
    }

    public void AddKills(EnemyData enemy, int count)
    {
        if (enemy == null || count <= 0) return;
        _killCounts.TryGetValue(enemy, out int current);
        _killCounts[enemy] = current + count;
        OnKillCountUpdated?.Invoke(enemy);
    }

    public void LoadKills(List<EnemyKillEntry> entries, SaveRegistry registry)
    {
        _killCounts.Clear();
        foreach (EnemyKillEntry entry in entries)
        {
            EnemyData enemy = registry.FindEnemy(entry.enemyName);
            if (enemy == null)
            {
                Debug.LogWarning($"[EnemyProgressTracker] Unknown enemy '{entry.enemyName}' in save — skipped.");
                continue;
            }
            if (entry.killCount > 0)
                _killCounts[enemy] = entry.killCount;
        }
    }

    #endregion

    #region Private Methods

    private void HandleEnemyKilled(EnemyData enemyData)
    {
        if (enemyData == null)
            return;
        _killCounts.TryGetValue(enemyData, out int current);
        _killCounts[enemyData] = current + 1;
        OnKillCountUpdated?.Invoke(enemyData);
    }

    #endregion
}
