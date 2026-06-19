using System;
using UnityEngine;

public class ResourceNodeInteraction : Interactable
{
    #region Private Fields

    private ResourceNode _node;
    private ResourceNodeHealth _health;
    private PlayerGathering _playerGathering;
    private PlayerLevel _playerLevel;
    private ClickIndicator _clickIndicator;
    private ClickRouter _clickRouter;
    private bool _isFollowing;
    private bool _isGathering;

    #endregion

    #region Static Events

    private static event Action<ResourceNodeInteraction> OnAnyResourceTargeted;

    #endregion

    #region Unity Lifecycle

    protected override void Start()
    {
        base.Start();
        if (!enabled)
            return;

        _node = GetComponent<ResourceNode>();
        _health = GetComponent<ResourceNodeHealth>();
        _playerGathering = FindFirstObjectByType<PlayerGathering>();
        _playerLevel = GameManager.Instance?.PlayerLevel;
        _clickIndicator = FindFirstObjectByType<ClickIndicator>();
        _clickRouter = GameManager.Instance?.ClickRouter;

        if (_node == null || _health == null || _playerGathering == null || _clickIndicator == null)
        {
            Debug.LogError("[ResourceNodeInteraction] Missing required dependency.");
            enabled = false;
            return;
        }

        if (_clickRouter != null)
            _clickRouter.OnGroundClicked += HandleGroundClicked;
        OnAnyResourceTargeted += HandleAnyResourceTargeted;
    }

    private void OnDisable()
    {
        CancelAll();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_clickRouter != null)
            _clickRouter.OnGroundClicked -= HandleGroundClicked;
        OnAnyResourceTargeted -= HandleAnyResourceTargeted;
    }

    #endregion

    #region Public Methods

    public override void OnInteract()
    {
        if (_node.Data == null)
            return;

        int required = _node.Data.RequiredLevel;
        if (required > 1 && _playerLevel != null && _playerLevel.Level < required)
        {
            DamagePopupSpawner.TrySpawnLevelRequired(transform.position, required);
            return;
        }

        ShowCursor();
        OnAnyResourceTargeted?.Invoke(this);
        BeginFollowing();
    }

    #endregion

    #region Private Methods

    private void BeginFollowing()
    {
        StopGathering();

        if (IsPlayerInRange() && !_playerMovement.IsMoving)
        {
            FaceNode();
            BeginGathering();
            return;
        }

        _isFollowing = true;
        MoveToNode();
    }

    private void MoveToNode()
    {
        float gatherRange = _playerGathering.GatherRange;
        float stopDist = Mathf.Max(0.1f, gatherRange - 0.5f);

        float approachSign = Mathf.Sign(
            _playerMovement.transform.position.x - transform.position.x
        );
        if (approachSign == 0f)
            approachSign = -1f;

        Vector2 approachPos = new(
            transform.position.x + approachSign * stopDist,
            transform.position.y
        );

        StartApproach(approachPos, gatherRange, OnPlayerArrived);
    }

    private void OnPlayerArrived()
    {
        if (!_isFollowing)
            return;

        _isFollowing = false;
        ShowCursor(); // re-show after movement-stopped hid it
        FaceNode();
        BeginGathering();
    }

    private void BeginGathering()
    {
        if (_isGathering)
            return;

        _isGathering = true;
        _playerMovement.OnMovementStarted += HandlePlayerMovedAway;
        _playerGathering.OnTargetOutOfRange += HandleOutOfRange;
        _playerGathering.StartGathering(_health);
    }

    private void StopGathering()
    {
        if (!_isGathering)
            return;

        _isGathering = false;
        _playerMovement.OnMovementStarted -= HandlePlayerMovedAway;
        if (_playerGathering != null)
        {
            _playerGathering.OnTargetOutOfRange -= HandleOutOfRange;
            _playerGathering.StopGathering();
        }
    }

    // Cancels following/gathering state without touching cursor — caller owns cursor state.
    private void CancelState()
    {
        _isFollowing = false;
        StopGathering();
        CancelApproach();
    }

    private void CancelAll()
    {
        CancelState();
        _clickIndicator?.HideCursor();
    }

    private void ShowCursor()
    {
        if (_node.Data.NodeType == ResourceNodeType.Mining)
            _clickIndicator.ShowMiningCursor(transform);
        else
            _clickIndicator.ShowWoodcuttingCursor(transform);
    }

    private void FaceNode()
    {
        float dirX = transform.position.x - _playerMovement.transform.position.x;
        if (Mathf.Abs(dirX) > 0.01f)
            _playerMovement.SetFacing(dirX > 0f);
    }

    private void HandleAnyResourceTargeted(ResourceNodeInteraction targeted)
    {
        if (targeted == this)
            return;
        if (_isFollowing || _isGathering)
            CancelState();
    }

    private void HandleGroundClicked(Vector2 _)
    {
        if (!_isFollowing && !_isGathering)
            return;

        // Don't hide cursor here — PlayerMovement's handler will show the ground cursor.
        CancelState();
    }

    private void HandlePlayerMovedAway()
    {
        // Player started moving toward something else — cancel without hiding cursor since the
        // new target manages its own cursor.
        CancelState();
    }

    private void HandleOutOfRange()
    {
        BeginFollowing();
    }

    #endregion

    #region Protected Overrides

    protected override bool IsPlayerInRange() =>
        Vector2.Distance(_playerMovement.transform.position, transform.position)
        <= _playerGathering.GatherRange;

    // No retry limit — keep following until the player gives up (ground click or other target).
    protected override void OnApproachFailed()
    {
        if (_isFollowing)
            MoveToNode();
    }

    #endregion
}
