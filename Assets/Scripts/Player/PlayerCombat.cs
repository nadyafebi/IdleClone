using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{
    #region Serialized Fields

    [Header("Combat")]
    [SerializeField]
    private int _attackDamage = 1;

    [SerializeField]
    [Tooltip("Seconds between attacks.")]
    private float _attackCooldown = 1f;

    [SerializeField]
    [Tooltip("Maximum distance to the enemy for attacks to land.")]
    private float _attackRange = 2f;

    #endregion

    #region Public Properties

    public bool IsAttacking => _attackCoroutine != null;
    public float AttackRange => _attackRange;

    public event Action OnTargetOutOfRange;
    public event Action OnTargetKilled;

    #endregion

    #region Private Fields

    private EnemyHealth _target;
    private Coroutine _attackCoroutine;
    private PlayerMovement _playerMovement;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        if (_playerMovement == null)
        {
            Debug.LogError("[PlayerCombat] No PlayerMovement found on this GameObject!");
            enabled = false;
        }
    }

    #endregion

    #region Public Methods

    public void StartAttacking(EnemyHealth target)
    {
        if (_target == target)
            return;

        StopAttacking();
        _target = target;
        _target.OnDied += HandleTargetDied;
        _attackCoroutine = StartCoroutine(AttackLoop());
    }

    public void StopAttacking()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
        if (_target != null)
        {
            _target.OnDied -= HandleTargetDied;
            _target = null;
        }
    }

    #endregion

    #region Combat

    private void HandleTargetDied()
    {
        StopAttacking();
        OnTargetKilled?.Invoke();
    }

    private IEnumerator AttackLoop()
    {
        while (_target != null)
        {
            float dirX = _target.transform.position.x - transform.position.x;
            bool outOfRange =
                Vector2.Distance(transform.position, _target.transform.position) > _attackRange;
            // 0.1f dead zone prevents re-follow flicker when the enemy is almost directly above/below.
            bool enemyBehind =
                Mathf.Abs(dirX) > 0.1f && (_playerMovement.FacingRight != (dirX > 0f));

            if (outOfRange || enemyBehind)
            {
                OnTargetOutOfRange?.Invoke();
                yield break;
            }

            _target.TakeDamage(_attackDamage);
            yield return new WaitForSeconds(_attackCooldown);
        }
    }

    #endregion
}
