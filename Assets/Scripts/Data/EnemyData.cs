using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropTableEntry
{
    #region Serialized Fields

    [SerializeField]
    private ItemData _item;

    [SerializeField]
    [Min(1)]
    private int _quantity = 1;

    [Tooltip("1 in X chance to drop. Set to 1 for guaranteed.")]
    [SerializeField]
    [Min(1)]
    private int _chance = 1;

    #endregion

    #region Public Properties

    public ItemData Item => _item;
    public int Quantity => _quantity;
    public int Chance => _chance;

    #endregion
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "IdleClone/Enemy Data")]
public class EnemyData : ScriptableObject
{
    #region Serialized Fields

    [Header("Drops")]
    [SerializeField]
    private List<DropTableEntry> _dropTable = new();

    #endregion

    #region Public Methods

    // Returns items that passed their drop roll for this kill.
    public List<(ItemData item, int quantity)> RollDrops()
    {
        var drops = new List<(ItemData, int)>();
        foreach (DropTableEntry entry in _dropTable)
        {
            if (entry.Item == null)
                continue;

            // chance == 1 is guaranteed; chance == 500 means 1-in-500
            if (UnityEngine.Random.Range(1, entry.Chance + 1) == 1)
            {
                drops.Add((entry.Item, entry.Quantity));
            }
        }
        return drops;
    }

    #endregion
}
