using UnityEngine;

public class ClassChangeOnDialogLine : MonoBehaviour
{
    #region Serialized Fields

    [Tooltip("The quest whose complete dialog triggers the class change.")]
    [SerializeField]
    private QuestData _triggerQuest;

    [Tooltip("1-based line number. Class change fires when the player advances past this line (e.g. 2 = fires after seeing line 2).")]
    [SerializeField]
    [Min(1)]
    private int _triggerAfterLine = 2;

    [SerializeField]
    private PlayerClass _targetClass = PlayerClass.Awakened;

    #endregion

    #region Private Fields

    private QuestManager _questManager;
    private DialogController _dialogController;
    private bool _triggered;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _questManager = GameManager.Instance != null ? GameManager.Instance.QuestManager : null;
        _dialogController = GameManager.Instance != null ? GameManager.Instance.DialogController : null;

        if (_questManager == null || _dialogController == null)
        {
            Debug.LogError("[ClassChangeOnDialogLine] Missing required dependency on GameManager.");
            enabled = false;
            return;
        }

        _dialogController.OnLineAdvanced += HandleLineAdvanced;
        _questManager.OnQuestUpdated += HandleQuestUpdated;
    }

    private void OnDestroy()
    {
        if (_dialogController != null)
            _dialogController.OnLineAdvanced -= HandleLineAdvanced;

        if (_questManager != null)
            _questManager.OnQuestUpdated -= HandleQuestUpdated;
    }

    #endregion

    #region Private Methods

    private void HandleLineAdvanced(int lineIndex)
    {
        if (_triggered || _triggerQuest == null)
            return;

        if (_questManager.GetState(_triggerQuest) != QuestState.Active)
            return;

        if (!_questManager.IsObjectiveMet(_triggerQuest))
            return;

        // lineIndex is 0-based; _triggerAfterLine is 1-based, so both equal at the target.
        if (lineIndex != _triggerAfterLine)
            return;

        _triggered = true;

        PlayerProgression progression = GameManager.Instance != null ? GameManager.Instance.PlayerProgression : null;
        if (progression != null)
            progression.ChangeClass(_targetClass);
        else
            Debug.LogError("[ClassChangeOnDialogLine] PlayerProgression not found on GameManager.");
    }

    private void HandleQuestUpdated(QuestData quest)
    {
        if (quest == _triggerQuest && _questManager.GetState(quest) == QuestState.Completed)
            _triggered = false;
    }

    #endregion
}
