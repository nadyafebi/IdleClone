using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    #region Protected Fields

    protected PlayerMovement _playerMovement;

    #endregion

    #region Private Fields

    private bool _isApproaching;
    private float _activeArrivalThreshold;
    private Action _onArrived;

    #endregion

    #region Unity Lifecycle

    protected virtual void Start()
    {
        _playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (_playerMovement == null)
        {
            Debug.LogError($"[{GetType().Name}] No PlayerMovement found in scene.");
            enabled = false;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_playerMovement != null)
            _playerMovement.OnMovementStopped -= HandleApproachArrived;
    }

    #endregion

    #region Public Methods

    public abstract void OnInteract();

    #endregion

    #region Protected Methods

    protected void StartApproach(Vector2 destination, float arrivalThreshold, Action onArrived)
    {
        _activeArrivalThreshold = arrivalThreshold;
        _onArrived = onArrived;

        if (!_isApproaching)
        {
            _isApproaching = true;
            _playerMovement.OnMovementStopped += HandleApproachArrived;
        }

        _playerMovement.MoveTo(destination);
    }

    #endregion

    #region Protected Methods

    protected virtual bool IsPlayerInRange() =>
        Vector2.Distance(_playerMovement.transform.position, transform.position) <= _activeArrivalThreshold;

    #endregion

    #region Private Methods

    private void HandleApproachArrived()
    {
        _playerMovement.OnMovementStopped -= HandleApproachArrived;
        _isApproaching = false;

        if (!IsPlayerInRange())
            return;

        _onArrived?.Invoke();
    }

    #endregion
}
