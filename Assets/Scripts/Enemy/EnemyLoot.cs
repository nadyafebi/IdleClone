using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(Enemy))]
public class EnemyLoot : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private DroppedItem _droppedItemPrefab;

    #endregion

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
            Debug.LogError("[EnemyLoot] Missing EnemyHealth component.");
            enabled = false;
            return;
        }
        if (_droppedItemPrefab == null)
        {
            Debug.LogError("[EnemyLoot] No DroppedItem prefab assigned.");
            enabled = false;
            return;
        }

        var enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("[EnemyLoot] Missing Enemy component.");
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
        if (_data == null)
            return;

        foreach (var (item, quantity) in _data.RollDrops())
        {
            DroppedItem dropped = Instantiate(
                _droppedItemPrefab,
                transform.position,
                Quaternion.identity
            );
            dropped.Initialize(item, quantity);
        }
    }

    #endregion
}
