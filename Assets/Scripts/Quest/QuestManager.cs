using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    NotStarted,
    Active,
    Completed,
}

public class QuestManager : MonoBehaviour
{
    #region Public Properties

    public event Action<QuestData> OnQuestUpdated;

    #endregion

    #region Private Fields

    private readonly Dictionary<QuestData, QuestState> _states = new();
    private readonly Dictionary<QuestData, int> _killCounts = new();

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        Enemy.OnAnyEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        Enemy.OnAnyEnemyKilled -= HandleEnemyKilled;
    }

    private void Start()
    {
        PlayerInventory inventory = GameManager.Instance?.PlayerInventory;
        if (inventory != null)
            inventory.OnChanged += HandleInventoryChanged;

        PlayerUpgrades upgrades = GameManager.Instance?.PlayerUpgrades;
        if (upgrades != null)
            upgrades.OnUpgraded += HandleUpgraded;
    }

    private void OnDestroy()
    {
        PlayerInventory inventory = GameManager.Instance?.PlayerInventory;
        if (inventory != null)
            inventory.OnChanged -= HandleInventoryChanged;

        PlayerUpgrades upgrades = GameManager.Instance?.PlayerUpgrades;
        if (upgrades != null)
            upgrades.OnUpgraded -= HandleUpgraded;
    }

    #endregion

    #region Public Methods

    public QuestState GetState(QuestData quest)
    {
        if (quest == null)
            return QuestState.Completed;

        _states.TryGetValue(quest, out QuestState state);
        return state;
    }

    public bool IsObjectiveMet(QuestData quest)
    {
        if (quest == null || GetState(quest) != QuestState.Active)
            return false;

        return quest.Type switch
        {
            QuestType.Kill => GetKillCount(quest) >= quest.RequiredCount,
            QuestType.Collect => GetCollectCount(quest) >= quest.RequiredCount,
            QuestType.Upgrade => GetUpgradeTier(quest) >= quest.RequiredCount,
            _ => false,
        };
    }

    public int GetCurrentCount(QuestData quest)
    {
        if (quest == null)
            return 0;

        return quest.Type switch
        {
            QuestType.Kill => GetKillCount(quest),
            QuestType.Collect => GetCollectCount(quest),
            QuestType.Upgrade => GetUpgradeTier(quest),
            _ => 0,
        };
    }

    public bool IsDependencyMet(QuestData quest)
    {
        if (quest == null || quest.Dependency == null)
            return true;

        return GetState(quest.Dependency) == QuestState.Completed;
    }

    public void StartQuest(QuestData quest)
    {
        if (quest == null || GetState(quest) != QuestState.NotStarted)
            return;

        _states[quest] = QuestState.Active;

        int initialKills = 0;
        if (quest.Type == QuestType.Kill && quest.TargetEnemy != null)
        {
            EnemyProgressTracker tracker =
                GameManager.Instance != null ? GameManager.Instance.EnemyProgressTracker : null;
            if (tracker != null)
                initialKills = Mathf.Min(
                    tracker.GetKillCount(quest.TargetEnemy),
                    quest.RequiredCount
                );
        }
        _killCounts[quest] = initialKills;

        Debug.Log($"[QuestManager] Started: {quest.QuestName}");
        OnQuestUpdated?.Invoke(quest);
    }

    public void CompleteQuest(QuestData quest)
    {
        if (quest == null || GetState(quest) != QuestState.Active)
            return;

        _states[quest] = QuestState.Completed;
        GrantRewards(quest);

        Debug.Log($"[QuestManager] Completed: {quest.QuestName}");
        OnQuestUpdated?.Invoke(quest);
    }

    public List<QuestData> GetActiveQuests()
    {
        var result = new List<QuestData>();
        foreach (var kvp in _states)
        {
            if (kvp.Value == QuestState.Active)
                result.Add(kvp.Key);
        }
        return result;
    }

    #endregion

    #region Private Methods

    private int GetKillCount(QuestData quest)
    {
        _killCounts.TryGetValue(quest, out int count);
        return count;
    }

    private int GetCollectCount(QuestData quest)
    {
        if (quest.TargetItem == null || GameManager.Instance?.PlayerInventory == null)
            return 0;

        GameManager.Instance.PlayerInventory.Items.TryGetValue(quest.TargetItem, out int count);
        return count;
    }

    private void HandleInventoryChanged()
    {
        foreach (var kvp in _states)
        {
            QuestData quest = kvp.Key;
            if (kvp.Value != QuestState.Active || quest.Type != QuestType.Collect)
                continue;

            OnQuestUpdated?.Invoke(quest);
        }
    }

    private int GetUpgradeTier(QuestData quest)
    {
        PlayerUpgrades upgrades = GameManager.Instance?.PlayerUpgrades;
        return upgrades != null ? upgrades.GetTier(quest.TargetUpgradeType) : 0;
    }

    private void HandleUpgraded()
    {
        foreach (var kvp in _states)
        {
            QuestData quest = kvp.Key;
            if (kvp.Value != QuestState.Active || quest.Type != QuestType.Upgrade)
                continue;

            OnQuestUpdated?.Invoke(quest);
        }
    }

    private void HandleEnemyKilled(EnemyData enemyData)
    {
        foreach (var kvp in _states)
        {
            QuestData quest = kvp.Key;
            if (kvp.Value != QuestState.Active)
                continue;
            if (quest.Type != QuestType.Kill || quest.TargetEnemy != enemyData)
                continue;

            _killCounts.TryGetValue(quest, out int current);
            _killCounts[quest] = current + 1;

            OnQuestUpdated?.Invoke(quest);
        }
    }

    private void GrantRewards(QuestData quest)
    {
        PlayerInventory inventory = GameManager.Instance?.PlayerInventory;
        PlayerLevel level = GameManager.Instance?.PlayerLevel;

        if (inventory != null)
        {
            foreach (ItemReward reward in quest.ItemRewards)
            {
                if (reward.Item != null && reward.Quantity > 0)
                    inventory.AddItem(reward.Item, reward.Quantity);
            }
        }

        if (level != null && quest.XpReward > 0)
            level.AddXp(quest.XpReward);
    }

    #endregion
}
