using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{
    #region Public Properties

    public bool IsAttacking => _attackCoroutine != null;
    public float AttackRange => _stats != null ? _stats.AttackRange : 0f;

    public event Action OnTargetOutOfRange;
    public event Action OnTargetKilled;

    #endregion

    #region Private Fields

    private EnemyHealth _target;
    private Coroutine _attackCoroutine;
    private PlayerMovement _playerMovement;
    private PlayerStats _stats;

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

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[PlayerCombat] No GameManager found!");
            enabled = false;
            return;
        }
        _stats = GameManager.Instance.PlayerStats;
        if (_stats == null)
        {
            Debug.LogError("[PlayerCombat] No PlayerStats found on GameManager!");
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
                Vector2.Distance(transform.position, _target.transform.position) > AttackRange;
            // 0.1f dead zone prevents re-follow flicker when the enemy is almost directly above/below.
            bool enemyBehind =
                Mathf.Abs(dirX) > 0.1f && (_playerMovement.FacingRight != (dirX > 0f));

            if (outOfRange || enemyBehind)
            {
                OnTargetOutOfRange?.Invoke();
                yield break;
            }

            _target.TakeDamage(_stats.TotalAttack);
            yield return new WaitForSeconds(_stats.AttackCooldown);
        }
    }

    #endregion
}
