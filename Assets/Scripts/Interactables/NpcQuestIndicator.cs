using UnityEngine;

public enum QuestIndicatorState { None, Available, Active, ReadyToTurnIn }

public class NpcQuestIndicator : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private SpriteRenderer _renderer;

    [Tooltip("Shown when the quest is available but not yet accepted (the '!' sprite).")]
    [SerializeField]
    private Sprite _spriteAvailable;

    [Tooltip("Shown when the quest is accepted, regardless of completion (the '?' sprite).")]
    [SerializeField]
    private Sprite _spriteAccepted;

    [SerializeField]
    private Color _colorAvailable = new(0.2f, 0.5f, 1f);

    [SerializeField]
    private Color _colorActive = new(0.6f, 0.6f, 0.6f);

    [SerializeField]
    private Color _colorReadyToTurnIn = new(0.2f, 0.9f, 0.3f);

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();
    }

    #endregion

    #region Public Methods

    public void SetState(QuestIndicatorState state)
    {
        if (_renderer == null)
            return;

        if (state == QuestIndicatorState.None)
        {
            _renderer.enabled = false;
            return;
        }

        _renderer.enabled = true;
        _renderer.sprite = state == QuestIndicatorState.Available ? _spriteAvailable : _spriteAccepted;
        _renderer.color = state switch
        {
            QuestIndicatorState.Available => _colorAvailable,
            QuestIndicatorState.Active => _colorActive,
            QuestIndicatorState.ReadyToTurnIn => _colorReadyToTurnIn,
            _ => Color.white
        };
    }

    #endregion
}
