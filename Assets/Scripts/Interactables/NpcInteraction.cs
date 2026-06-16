using UnityEngine;

public class NpcInteraction : Interactable
{
    #region Serialized Fields

    [SerializeField]
    private DialogData _dialogData;

    [Tooltip("How far from the NPC the player stops (world units).")]
    [SerializeField]
    private float _approachDistance = 1.5f;

    #endregion

    #region Private Fields

    private PlayerMovement _playerMovement;
    private DialogController _dialogController;
    private ClickIndicator _clickIndicator;
    private bool _dialogOpen;
    private bool _approachingNpc;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _playerMovement = FindFirstObjectByType<PlayerMovement>();
        _dialogController = FindFirstObjectByType<DialogController>();
        _clickIndicator = FindFirstObjectByType<ClickIndicator>();

        if (_playerMovement == null || _dialogController == null || _clickIndicator == null)
        {
            Debug.LogError("[NpcInteraction] Missing required scene dependency.");
            enabled = false;
            return;
        }
    }

    private void OnDestroy()
    {
        if (_playerMovement != null)
            _playerMovement.OnMovementStopped -= HandlePlayerArrived;
    }

    #endregion

    #region Public Methods

    public override void OnInteract()
    {
        bool isNear = IsPlayerNear();

        if (_dialogOpen)
        {
            if (!isNear)
            {
                // Player walked away during dialog — move back first, advance on arrival.
                StartApproach();
                return;
            }
            _dialogController.Advance();
            return;
        }

        if (isNear && !_playerMovement.IsMoving)
        {
            // Already standing next to the NPC — open dialog with no movement or cursor.
            OpenDialog();
            return;
        }

        StartApproach();
    }

    #endregion

    #region Private Methods

    private void StartApproach()
    {
        _clickIndicator.ShowNpcCursor(transform);

        if (!_approachingNpc)
        {
            _approachingNpc = true;
            _playerMovement.OnMovementStopped += HandlePlayerArrived;
        }

        float approachSign = Mathf.Sign(_playerMovement.transform.position.x - transform.position.x);
        if (approachSign == 0f)
            approachSign = -1f;

        Vector2 approachPos = new(
            transform.position.x + approachSign * _approachDistance,
            transform.position.y
        );
        _playerMovement.MoveTo(approachPos);
    }

    private void HandlePlayerArrived()
    {
        _playerMovement.OnMovementStopped -= HandlePlayerArrived;
        _approachingNpc = false;

        if (!IsPlayerNear())
            return;

        if (_dialogOpen)
            _dialogController.Advance();
        else
            OpenDialog();
    }

    private void OpenDialog()
    {
        _dialogOpen = true;
        _dialogController.Open(_dialogData, transform, () => _dialogOpen = false);
    }

    private bool IsPlayerNear() =>
        Vector2.Distance(_playerMovement.transform.position, transform.position) <= _approachDistance + 0.5f;

    #endregion
}
