using System.Collections.Generic;
using UnityEngine;

public class NpcQuestGiver : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private List<QuestData> _quests = new();

    [Tooltip("Shown when all quests are completed.")]
    [SerializeField]
    private DialogData _idleDialog;

    [SerializeField]
    private NpcQuestIndicator _indicator;

    #endregion

    #region Private Fields

    private QuestManager _questManager;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _questManager = GameManager.Instance?.QuestManager;
        if (_questManager == null)
        {
            Debug.LogError("[NpcQuestGiver] QuestManager not found on GameManager.");
            enabled = false;
            return;
        }

        _questManager.OnQuestUpdated += HandleQuestUpdated;
        RefreshIndicator();
    }

    private void OnDestroy()
    {
        if (_questManager != null)
            _questManager.OnQuestUpdated -= HandleQuestUpdated;
    }

    #endregion

    #region Public Methods

    public DialogData GetCurrentDialog()
    {
        QuestData quest = GetCurrentQuest();

        if (quest == null)
            return _idleDialog;

        if (!_questManager.IsDependencyMet(quest))
            return quest.LockedDialog;

        QuestState state = _questManager.GetState(quest);

        if (state == QuestState.NotStarted)
            return quest.OfferDialog;

        if (state == QuestState.Active)
        {
            return _questManager.IsObjectiveMet(quest)
                ? quest.CompleteDialog
                : quest.ActiveDialog;
        }

        return _idleDialog;
    }

    // Returns true when a quest was just completed and the next quest is immediately offerable.
    public bool OnDialogClosed()
    {
        QuestData quest = GetCurrentQuest();
        if (quest == null || !_questManager.IsDependencyMet(quest))
            return false;

        QuestState state = _questManager.GetState(quest);
        bool questJustCompleted = false;

        if (state == QuestState.NotStarted)
            _questManager.StartQuest(quest);
        else if (state == QuestState.Active && _questManager.IsObjectiveMet(quest))
        {
            _questManager.CompleteQuest(quest);
            questJustCompleted = true;
        }

        RefreshIndicator();

        if (!questJustCompleted)
            return false;

        QuestData nextQuest = GetCurrentQuest();
        return nextQuest != null
            && _questManager.IsDependencyMet(nextQuest)
            && _questManager.GetState(nextQuest) == QuestState.NotStarted;
    }

    public QuestIndicatorState GetIndicatorState()
    {
        QuestData quest = GetCurrentQuest();

        if (quest == null || !_questManager.IsDependencyMet(quest))
            return QuestIndicatorState.None;

        QuestState state = _questManager.GetState(quest);

        if (state == QuestState.NotStarted)
            return QuestIndicatorState.Available;

        if (state == QuestState.Active)
        {
            return _questManager.IsObjectiveMet(quest)
                ? QuestIndicatorState.ReadyToTurnIn
                : QuestIndicatorState.Active;
        }

        return QuestIndicatorState.None;
    }

    #endregion

    #region Private Methods

    private QuestData GetCurrentQuest()
    {
        foreach (QuestData quest in _quests)
        {
            if (_questManager.GetState(quest) != QuestState.Completed)
                return quest;
        }
        return null;
    }

    private void RefreshIndicator()
    {
        _indicator?.SetState(GetIndicatorState());
    }

    private void HandleQuestUpdated(QuestData quest)
    {
        if (_quests.Contains(quest))
            RefreshIndicator();
    }

    #endregion
}
