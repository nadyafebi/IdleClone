using UnityEngine;

public class ClickIndicator : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private ClickRouter _router;

    [SerializeField]
    private PlayerMovement _movement;

    [SerializeField]
    private SpriteRenderer _cursor;

    [Header("Ground Cursor")]
    [SerializeField]
    private Sprite _groundSprite;

    [SerializeField]
    private Vector2 _groundCursorOffset;

    [Header("Interactable Cursor Sprites")]
    [SerializeField]
    private Sprite _enemySprite;

    [SerializeField]
    private Sprite _npcSprite;

    #endregion

    #region Private Fields

    private Transform _trackedTarget;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_router != null)
        {
            _router.OnEnemyClicked += HandleEnemyClicked;
        }

        if (_movement != null)
        {
            _movement.OnDestinationSet += HandleDestinationSet;
            _movement.OnMovementStopped += HandleMovementStopped;
        }
    }

    private void OnDisable()
    {
        if (_router != null)
        {
            _router.OnEnemyClicked -= HandleEnemyClicked;
        }

        if (_movement != null)
        {
            _movement.OnDestinationSet -= HandleDestinationSet;
            _movement.OnMovementStopped -= HandleMovementStopped;
        }
    }

    private void Start()
    {
        if (_router == null)
        {
            Debug.LogError("[ClickIndicator] No ClickRouter reference assigned!");
            enabled = false;
            return;
        }
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
            _cursor.transform.position = _trackedTarget.position;
    }

    #endregion

    #region Event Handlers

    private void HandleDestinationSet(Vector2 exactDest)
    {
        _trackedTarget = null;
        ShowCursor(_groundSprite, exactDest + _groundCursorOffset);
    }

    private void HandleMovementStopped()
    {
        _cursor.enabled = false;
    }

    private void HandleEnemyClicked(GameObject enemy)
    {
        _trackedTarget = enemy.transform;
        ShowCursor(_enemySprite, enemy.transform.position);
    }

    #endregion

    #region Public Methods

    public void ShowNpcCursor(Transform npc)
    {
        _trackedTarget = npc;
        ShowCursor(_npcSprite, npc.position);
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
