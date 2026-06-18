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

    [Header("Identity")]
    [SerializeField]
    private string _enemyName = "Enemy";

    [SerializeField]
    [Min(1)]
    private int _maxHealth = 10;

    [SerializeField]
    private Sprite _sprite;

    [SerializeField]
    private RuntimeAnimatorController _animatorController;

    [Header("Spawning")]
    [SerializeField]
    [Tooltip("Seconds before a dead enemy respawns.")]
    private float _respawnCooldown = 10f;

    [Header("Collider")]
    [SerializeField]
    private Vector2 _colliderSize = Vector2.one;

    [SerializeField]
    private Vector2 _colliderOffset = Vector2.zero;

    [Header("Rewards")]
    [SerializeField]
    [Min(0)]
    private int _xpReward = 10;

    [Header("Drops")]
    [SerializeField]
    private List<DropTableEntry> _dropTable = new();

    #endregion

    #region Public Properties

    public string EnemyName => _enemyName;
    public int MaxHealth => _maxHealth;
    public Sprite Sprite => _sprite;
    public RuntimeAnimatorController AnimatorController => _animatorController;
    public float RespawnCooldown => _respawnCooldown;
    public Vector2 ColliderSize => _colliderSize;
    public Vector2 ColliderOffset => _colliderOffset;
    public int XpReward => _xpReward;

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
