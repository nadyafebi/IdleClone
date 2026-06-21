using System;
using UnityEngine;

public class EnemyInteraction : Interactable
{
    #region Private Fields

    private EnemyHealth _health;
    private EnemyMovement _enemyMovement;
    private PlayerCombat _playerCombat;
    private ClickIndicator _clickIndicator;
    private ClickRouter _clickRouter;
    private bool _isFollowing;
    private bool _isTargeting;

    #endregion

    #region Static Events

    private static event Action<EnemyInteraction> OnAnyEnemyTargeted;

    #endregion

    #region Unity Lifecycle

    protected override void Start()
    {
        base.Start();
        if (!enabled)
            return;

        _health = GetComponent<EnemyHealth>();
        _enemyMovement = GetComponent<EnemyMovement>();
        _playerCombat = FindFirstObjectByType<PlayerCombat>();
        _clickIndicator = FindFirstObjectByType<ClickIndicator>();
        _clickRouter = GameManager.Instance?.ClickRouter;

        if (
            _health == null
            || _enemyMovement == null
            || _playerCombat == null
            || _clickIndicator == null
        )
        {
            Debug.LogError("[EnemyInteraction] Missing required dependency.");
            enabled = false;
            return;
        }

        _health.OnDied += HandleDied;
        if (_clickRouter != null)
            _clickRouter.OnGroundClicked += HandleGroundClicked;
        OnAnyEnemyTargeted += HandleAnyEnemyTargeted;
    }

    private void OnDisable()
    {
        CancelAll();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_health != null)
            _health.OnDied -= HandleDied;
        if (_clickRouter != null)
            _clickRouter.OnGroundClicked -= HandleGroundClicked;
        OnAnyEnemyTargeted -= HandleAnyEnemyTargeted;
    }

    #endregion

    #region Public Methods

    public override void OnInteract()
    {
        if (_health.CurrentHealth <= 0)
            return;

        _clickIndicator.ShowEnemyCursor(transform);
        OnAnyEnemyTargeted?.Invoke(this);
        BeginFollowing();
    }

    #endregion

    #region Private Methods

    private void BeginFollowing()
    {
        StopTargeting();

        // Already in range and standing still — face the enemy and start attacking directly.
        if (IsPlayerInRange() && !_playerMovement.IsMoving)
        {
            FaceEnemy();
            BeginAttacking();
            return;
        }

        _isFollowing = true;
        MoveToEnemy();
    }

    private void MoveToEnemy()
    {
        float attackRange = _playerCombat.AttackRange;
        float stopDist = Mathf.Max(0.1f, attackRange - 0.5f);

        float approachSign = Mathf.Sign(
            _playerMovement.transform.position.x - transform.position.x
        );
        if (approachSign == 0f)
            approachSign = -1f;

        Vector2 approachPos = new(
            transform.position.x + approachSign * stopDist,
            transform.position.y
        );

        StartApproach(approachPos, attackRange, OnPlayerArrived);
    }

    private void OnPlayerArrived()
    {
        if (!_isFollowing)
            return;

        _isFollowing = false;
        _clickIndicator.ShowEnemyCursor(transform); // re-show after movement-stopped hid it
        FaceEnemy();
        BeginAttacking();
    }

    private void BeginAttacking()
    {
        if (_isTargeting)
            return;

        _isTargeting = true;
        _playerMovement.OnMovementStarted += HandlePlayerMovedAway;
        _enemyMovement.OnMovementStarted += HandleEnemyMovedAway;
        _playerCombat.OnTargetOutOfRange += HandleOutOfRange;
        _playerCombat.StartAttacking(_health);
    }

    private void StopTargeting()
    {
        if (!_isTargeting)
            return;

        _isTargeting = false;
        _playerMovement.OnMovementStarted -= HandlePlayerMovedAway;
        _enemyMovement.OnMovementStarted -= HandleEnemyMovedAway;
        if (_playerCombat != null)
        {
            _playerCombat.OnTargetOutOfRange -= HandleOutOfRange;
            _playerCombat.StopAttacking();
        }
    }

    // Cancels following/attacking state without touching cursor — caller owns cursor state.
    private void CancelState()
    {
        _isFollowing = false;
        StopTargeting();
        CancelApproach();
    }

    private void CancelAll()
    {
        CancelState();
        _clickIndicator?.HideCursor();
    }

    private void HandleAnyEnemyTargeted(EnemyInteraction targeted)
    {
        if (targeted == this)
            return;
        if (_isFollowing || _isTargeting)
            CancelState();
    }

    private void HandleGroundClicked(Vector2 _)
    {
        if (!_isFollowing && !_isTargeting)
            return;

        // Don't hide the cursor here — PlayerMovement's handler will show the ground cursor.
        CancelState();
        if (_playerCombat != null)
            _playerCombat.ClearLastTarget();
    }

    private void HandlePlayerMovedAway()
    {
        // Player started moving toward something else — cancel without hiding cursor since the
        // new target manages its own cursor.
        CancelState();
        if (_playerCombat != null)
            _playerCombat.ClearLastTarget();
    }

    private void HandleEnemyMovedAway()
    {
        // Enemy patrol started while player was attacking — resume following.
        BeginFollowing();
    }

    private void HandleOutOfRange()
    {
        // PlayerCombat detected the enemy walked beyond attack range mid-attack.
        BeginFollowing();
    }

    private void HandleDied()
    {
        CancelAll();
    }

    #endregion

    #region Protected Methods

    protected override bool IsPlayerInRange() =>
        Vector2.Distance(_playerMovement.transform.position, transform.position)
        <= _playerCombat.AttackRange;

    private void FaceEnemy()
    {
        float dirX = transform.position.x - _playerMovement.transform.position.x;
        if (Mathf.Abs(dirX) > 0.01f)
            _playerMovement.SetFacing(dirX > 0f);
    }

    // No retry limit — keep following until the player gives up (ground click) or enemy dies.
    protected override void OnApproachFailed()
    {
        if (_isFollowing)
            MoveToEnemy();
    }

    #endregion
}
