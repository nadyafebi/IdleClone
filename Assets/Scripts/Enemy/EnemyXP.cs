using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(Enemy))]
public class EnemyXP : MonoBehaviour
{
    #region Private Fields

    private EnemyHealth _health;
    private EnemyData _data;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        if (_health == null)
        {
            Debug.LogError("[EnemyXP] Missing EnemyHealth component.");
            enabled = false;
            return;
        }

        var enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("[EnemyXP] Missing Enemy component.");
            enabled = false;
            return;
        }

        _data = enemy.Data;
        _health.OnDied += HandleDied;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDied -= HandleDied;
    }

    #endregion

    #region Private Methods

    private void HandleDied()
    {
        if (_data == null || GameManager.Instance == null)
            return;

        PlayerLevel playerLevel = GameManager.Instance.PlayerLevel;
        if (playerLevel == null)
            return;

        playerLevel.AddXp(_data.XpReward);
    }

    #endregion
}
