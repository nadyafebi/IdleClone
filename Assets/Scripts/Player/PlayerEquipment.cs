using System;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    #region Public Properties

    public ItemData WeaponSlot { get; private set; }
    public ItemData ShieldSlot { get; private set; }
    public ItemData PotionSlot { get; private set; }
    public int PotionSlotQuantity { get; private set; }

    public int TotalAttackBonus => WeaponSlot != null ? WeaponSlot.AttackBonus : 0;
    public int TotalDefenseBonus => ShieldSlot != null ? ShieldSlot.DefenseBonus : 0;

    public event Action OnEquipmentChanged;

    #endregion

    #region Public Methods

    public void Equip(ItemData item)
    {
        switch (item.Category)
        {
            case ItemCategory.Weapon: WeaponSlot = item; break;
            case ItemCategory.Shield: ShieldSlot = item; break;
            default: return;
        }
        OnEquipmentChanged?.Invoke();
    }

    // Stacks onto existing slot if same item, replaces otherwise.
    public void EquipPotion(ItemData item, int quantity)
    {
        if (PotionSlot == item)
            PotionSlotQuantity += quantity;
        else
        {
            PotionSlot = item;
            PotionSlotQuantity = quantity;
        }
        OnEquipmentChanged?.Invoke();
    }

    // Decrements quantity by 1; clears the slot when it reaches 0.
    public void ConsumePotion()
    {
        if (PotionSlot == null)
            return;
        PotionSlotQuantity--;
        if (PotionSlotQuantity <= 0)
        {
            PotionSlot = null;
            PotionSlotQuantity = 0;
        }
        OnEquipmentChanged?.Invoke();
    }

    public void Unequip(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Weapon: WeaponSlot = null; break;
            case ItemCategory.Shield: ShieldSlot = null; break;
            case ItemCategory.Potion:
                PotionSlot = null;
                PotionSlotQuantity = 0;
                break;
            default: return;
        }
        OnEquipmentChanged?.Invoke();
    }

    public ItemData GetSlot(ItemCategory category) => category switch
    {
        ItemCategory.Weapon => WeaponSlot,
        ItemCategory.Shield => ShieldSlot,
        ItemCategory.Potion => PotionSlot,
        _ => null,
    };

    #endregion
}
