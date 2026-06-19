using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType { Kill, Collect }

[Serializable]
public class ItemReward
{
    [SerializeField]
    public ItemData Item;

    [SerializeField]
    [Min(1)]
    public int Quantity = 1;
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "IdleClone/Quest")]
public class QuestData : ScriptableObject
{
    #region Serialized Fields

    [Header("Identity")]
    [SerializeField]
    public string QuestName;

    [Header("Objective")]
    [SerializeField]
    public QuestType Type;

    [Tooltip("Enemy to kill (Kill type only).")]
    [SerializeField]
    public EnemyData TargetEnemy;

    [Tooltip("Item to collect (Collect type only).")]
    [SerializeField]
    public ItemData TargetItem;

    [SerializeField]
    [Min(1)]
    public int RequiredCount = 1;

    [Header("Dialogs")]
    [SerializeField]
    public DialogData OfferDialog;

    [Tooltip("Shown when re-talking while the quest is active but objective not met.")]
    [SerializeField]
    public DialogData ActiveDialog;

    [Tooltip("Shown when the objective is met and the quest is ready to turn in.")]
    [SerializeField]
    public DialogData CompleteDialog;

    [Tooltip("Shown when the dependency quest is not yet completed.")]
    [SerializeField]
    public DialogData LockedDialog;

    [Header("Rewards")]
    [SerializeField]
    public List<ItemReward> ItemRewards = new();

    [SerializeField]
    [Min(0)]
    public int XpReward;

    [Header("Dependency")]
    [Tooltip("This quest is locked until the referenced quest is completed.")]
    [SerializeField]
    public QuestData Dependency;

    #endregion
}
