using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProgressTracker : MonoBehaviour
{
    #region Public Properties

    public event Action<EnemyData> OnKillCountUpdated;

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
