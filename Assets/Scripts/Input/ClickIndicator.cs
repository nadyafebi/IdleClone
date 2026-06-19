using UnityEngine;

public class ClickIndicator : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private PlayerMovement _movement;

    [SerializeField]
    private SpriteRenderer _cursor;

    [Header("Ground Cursor")]
    [SerializeField]
    private Sprite _groundSprite;

    [SerializeField]
    private Vector2 _groundCursorOffset;

    [Header("Interactable Cursor")]
    [SerializeField]
    private Vector2 _interactableCursorOffset;

    [SerializeField]
    private Sprite _npcSprite;

    [SerializeField]
    private Sprite _portalSprite;

    [SerializeField]
    private Sprite _enemySprite;

    [SerializeField]
    private Sprite _miningSprite;

    [SerializeField]
    private Sprite _woodcuttingSprite;

    #endregion

    #region Private Fields

    private Transform _trackedTarget;
    private Vector2 _trackedOffset;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_movement != null)
        {
            _movement.OnDestinationSet += HandleDestinationSet;
            _movement.OnMovementStopped += HandleMovementStopped;
        }
    }

    private void OnDisable()
    {
        if (_movement != null)
        {
            _movement.OnDestinationSet -= HandleDestinationSet;
            _movement.OnMovementStopped -= HandleMovementStopped;
        }
    }

    private void Start()
    {
        if (_movement == null)
        {
            Debug.LogError("[ClickIndicator] No PlayerMovement reference assigned!");
            enabled = false;
            return;
        }
        if (_cursor == null)
        {
            Debug.LogError("[ClickIndicator] No SpriteRenderer reference assigned!");
            enabled = false;
            return;
        }

        _cursor.enabled = false;
    }

    private void Update()
    {
        if (_trackedTarget != null)
            _cursor.transform.position = (Vector2)_trackedTarget.position + _trackedOffset;
    }

    #endregion

    #region Event Handlers

    private void HandleDestinationSet(Vector2 exactDest)
    {
        _trackedTarget = null;
        _trackedOffset = Vector2.zero;
        ShowCursor(_groundSprite, exactDest + _groundCursorOffset);
    }

    private void HandleMovementStopped()
    {
        _cursor.enabled = false;
    }

    #endregion

    #region Public Methods

    public void ShowNpcCursor(Transform npc)
    {
        _trackedTarget = npc;
        _trackedOffset = _interactableCursorOffset;
        ShowCursor(_npcSprite, (Vector2)npc.position + _interactableCursorOffset);
    }

    public void ShowPortalCursor(Transform portal)
    {
        _trackedTarget = portal;
        _trackedOffset = _interactableCursorOffset;
        ShowCursor(_portalSprite, (Vector2)portal.position + _interactableCursorOffset);
    }

    public void ShowEnemyCursor(Transform enemy)
    {
        _trackedTarget = enemy;
        _trackedOffset = _interactableCursorOffset;
        ShowCursor(_enemySprite, (Vector2)enemy.position + _interactableCursorOffset);
    }

    public void ShowMiningCursor(Transform node)
    {
        _trackedTarget = node;
        _trackedOffset = _interactableCursorOffset;
        ShowCursor(_miningSprite, (Vector2)node.position + _interactableCursorOffset);
    }

    public void ShowWoodcuttingCursor(Transform node)
    {
        _trackedTarget = node;
        _trackedOffset = _interactableCursorOffset;
        ShowCursor(_woodcuttingSprite, (Vector2)node.position + _interactableCursorOffset);
    }

    public void HideCursor()
    {
        _trackedTarget = null;
        _trackedOffset = Vector2.zero;
        if (_cursor != null)
            _cursor.enabled = false;
    }

    #endregion

    #region Private Helpers

    private void ShowCursor(Sprite sprite, Vector2 position)
    {
        _cursor.sprite = sprite;
        _cursor.transform.position = position;
        _cursor.enabled = true;
    }

    #endregion
}
