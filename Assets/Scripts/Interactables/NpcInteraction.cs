using UnityEngine;

public class NpcInteraction : Interactable
{
    #region Serialized Fields

    [Tooltip("How far from the NPC the player stops (world units).")]
    [SerializeField]
    private float _approachDistance = 1.5f;

    #endregion

    #region Private Fields

    private DialogController _dialogController;
    private ClickIndicator _clickIndicator;
    private NpcQuestGiver _questGiver;
    private bool _dialogOpen;

    #endregion

    #region Unity Lifecycle

    protected override void Start()
    {
        base.Start();
        if (!enabled)
            return;

        _dialogController =
            GameManager.Instance != null ? GameManager.Instance.DialogController : null;
        _clickIndicator = FindFirstObjectByType<ClickIndicator>();

        if (_dialogController == null || _clickIndicator == null)
        {
            Debug.LogError("[NpcInteraction] Missing required scene dependency.");
            enabled = false;
            return;
        }

        _questGiver = GetComponent<NpcQuestGiver>();
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
                MoveToNpc();
                return;
            }
            _dialogController.Advance();
            return;
        }

        if (isNear && !_playerMovement.IsMoving)
        {
            OpenDialog();
            return;
        }

        MoveToNpc();
    }

    #endregion

    #region Private Methods

    private void MoveToNpc()
    {
        _clickIndicator.ShowNpcCursor(transform);

        float approachSign = Mathf.Sign(
            _playerMovement.transform.position.x - transform.position.x
        );
        if (approachSign == 0f)
            approachSign = -1f;

        Vector2 approachPos = new(
            transform.position.x + approachSign * _approachDistance,
            transform.position.y
        );

        StartApproach(approachPos, _approachDistance + 0.5f, OnPlayerArrived);
    }

    private void OnPlayerArrived()
    {
        if (_dialogOpen)
            _dialogController.Advance();
        else
            OpenDialog();
    }

    private void OpenDialog()
    {
        if (_questGiver == null)
            return;

        DialogData dialog = _questGiver.GetCurrentDialog();
        if (dialog == null)
            return;

        _dialogOpen = true;
        _dialogController.Open(dialog, transform, () =>
        {
            bool openNextQuest = _questGiver.OnDialogClosed();
            _dialogOpen = false;
            if (openNextQuest)
                OpenDialog();
        });
    }

    private bool IsPlayerNear() =>
        Vector2.Distance(_playerMovement.transform.position, transform.position)
        <= _approachDistance + 0.5f;

    #endregion
}
