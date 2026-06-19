using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "IdleClone/Item")]
public class ItemData : ScriptableObject
{
    #region Serialized Fields

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private Sprite _icon;

    [Header("Category")]
    [SerializeField]
    private ItemCategory _category;

    [Header("Effects")]
    [Tooltip("Weapon only: bonus added to base attack.")]
    [SerializeField]
    private int _attackBonus;

    [Tooltip("Shield only: bonus added to base defense.")]
    [SerializeField]
    private int _defenseBonus;

    [Tooltip("Potion only: HP restored on use.")]
    [SerializeField]
    private int _healAmount;

    #endregion

    #region Public Properties

    public string DisplayName => _displayName;
    public Sprite Icon => _icon;
    public ItemCategory Category => _category;
    public int AttackBonus => _attackBonus;
    public int DefenseBonus => _defenseBonus;
    public int HealAmount => _healAmount;

    #endregion
}
