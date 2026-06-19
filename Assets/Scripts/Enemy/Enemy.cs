using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private EnemyData _data;

    #endregion

    #region Public Properties

    public static event Action<EnemyData> OnAnyEnemyKilled;

    public EnemyData Data => _data;

    #endregion

    #region Private Fields

    private EnemyHealth _health;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        if (_health != null)
            _health.OnDied += HandleDied;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDied -= HandleDied;
    }

    #endregion

    #region Public Methods

    public void SetData(EnemyData data)
    {
        _data = data;

        var health = GetComponent<EnemyHealth>();
        if (health != null)
            health.SetMaxHealth(data.MaxHealth);

        var renderer = GetComponent<EnemyRenderer>();
        if (renderer != null)
            renderer.ApplyData(data);

        var col = GetComponent<Collider2D>();
        if (col is CapsuleCollider2D capsule)
        {
            capsule.size = data.ColliderSize;
            capsule.offset = data.ColliderOffset;
        }
        else if (col is BoxCollider2D box)
        {
            box.size = data.ColliderSize;
            box.offset = data.ColliderOffset;
        }
    }

    #endregion

    #region Private Methods

    private void HandleDied()
    {
        if (_data != null)
            OnAnyEnemyKilled?.Invoke(_data);
    }

    #endregion
}
