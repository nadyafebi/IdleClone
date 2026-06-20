using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(Enemy))]
public class EnemyAttack : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("Seconds between attacks.")]
    private float _attackCooldown = 1.5f;

    [SerializeField]
    [Tooltip("Layer mask for the player collider.")]
    private LayerMask _playerLayer;

    #endregion

    #region Private Fields

    private EnemyHealth _health;
    private EnemyData _data;
    private Collider2D _collider;
    private ContactFilter2D _playerFilter;
    private readonly Collider2D[] _overlapResults = new Collider2D[1];
    private float _nextAttackTime;
    private PlayerMovement _playerMovement;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (_health == null)
        {
            Debug.LogError("[EnemyAttack] Missing EnemyHealth component.");
            enabled = false;
            return;
        }
        if (_collider == null)
        {
            Debug.LogError("[EnemyAttack] Missing Collider2D component.");
            enabled = false;
            return;
        }

        var enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("[EnemyAttack] Missing Enemy component.");
            enabled = false;
            return;
        }

        _data = enemy.Data;
        _playerMovement = FindFirstObjectByType<PlayerMovement>();

        _playerFilter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = _playerLayer,
        };

        _health.OnDied += HandleDied;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDied -= HandleDied;
    }

    private void Update()
    {
        if (_health.CurrentHealth <= 0)
            return;

        if (Physics2D.OverlapCollider(_collider, _playerFilter, _overlapResults) == 0)
            return;

        if (Time.time < _nextAttackTime)
            return;

        _nextAttackTime = Time.time + _attackCooldown;

        if (GameManager.Instance == null)
            return;

        PlayerHealth playerHealth = GameManager.Instance.PlayerHealth;
        if (playerHealth != null)
        {
            int defense = GameManager.Instance.PlayerEquipment.TotalDefenseBonus;
            int finalDamage = Mathf.Max(0, _data.AttackDamage - defense);
            playerHealth.TakeDamage(finalDamage);

            if (_playerMovement != null)
                DamagePopupSpawner.TrySpawnEnemyDamage(
                    _playerMovement.transform.position,
                    finalDamage
                );
        }
    }

    #endregion

    #region Private Methods

    private void HandleDied()
    {
        _nextAttackTime = 0f;
    }

    #endregion
}
