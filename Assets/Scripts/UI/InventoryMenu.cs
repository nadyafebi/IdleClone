using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryMenu : GameMenu
{
    #region Private Fields

    private const int SlotCount = 16;

    private VisualElement _panel;
    private VisualElement _grid;
    private PlayerInventory _inventory;
    private PlayerEquipment _equipment;

    private VisualElement _equipmentPanel;
    private VisualElement _slotWeapon;
    private VisualElement _slotShield;
    private VisualElement _slotPotion;

    #endregion

    #region Protected Properties

    protected override VisualElement BlockingElement => _panel;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _inventory = GameManager.Instance.PlayerInventory;
        _equipment = GameManager.Instance.PlayerEquipment;

        _panel = Root.Q("inventory-panel");
        _grid = Root.Q("inventory-grid");

        _equipmentPanel = Root.Q("equipment-panel");
        _slotWeapon = Root.Q("slot-weapon");
        _slotShield = Root.Q("slot-shield");
        _slotPotion = Root.Q("slot-potion");

        RegisterEquipSlotClick(_slotWeapon, ItemCategory.Weapon);
        RegisterEquipSlotClick(_slotShield, ItemCategory.Shield);
        RegisterEquipSlotClick(_slotPotion, ItemCategory.Potion);
    }

    #endregion

    #region Public Methods

    public override bool ContainsScreenPoint(Vector2 screenPos)
    {
        if (!IsVisible)
            return false;
        return ElementContainsScreenPoint(_panel, screenPos)
            || ElementContainsScreenPoint(_equipmentPanel, screenPos);
    }

    #endregion

    #region Protected Methods

    protected override void OnShow()
    {
        _inventory.OnChanged += Rebuild;
        _equipment.OnEquipmentChanged += RefreshEquipmentSlots;
        Rebuild();
        RefreshEquipmentSlots();
    }

    protected override void OnHide()
    {
        _inventory.OnChanged -= Rebuild;
        _equipment.OnEquipmentChanged -= RefreshEquipmentSlots;
        GameManager.Instance.ItemTooltip.Hide();
    }

    #endregion

    #region Private Helpers

    private void Rebuild()
    {
        _grid.Clear();
        var items = new List<KeyValuePair<ItemData, int>>(_inventory.Items);

        for (int i = 0; i < SlotCount; i++)
        {
            var slot = new VisualElement();
            slot.AddToClassList("inventory-slot");

            if (i < items.Count)
            {
                ItemData item = items[i].Key;

                var icon = new VisualElement();
                icon.AddToClassList("slot-icon");
                icon.style.backgroundImage = new StyleBackground(item.Icon);

                var label = new Label(FormatQuantity(items[i].Value));
                label.AddToClassList("slot-quantity");

                slot.Add(icon);
                slot.Add(label);

                GameManager.Instance.ItemTooltip.RegisterHover(slot, item);

                if (item.Category != ItemCategory.Material)
                    slot.RegisterCallback<ClickEvent>(_ => EquipItem(item));
            }

            _grid.Add(slot);
        }
    }

    private void RefreshEquipmentSlots()
    {
        UpdateEquipSlot(_slotWeapon, _equipment.WeaponSlot);
        UpdateEquipSlot(_slotShield, _equipment.ShieldSlot);
        UpdateEquipSlot(_slotPotion, _equipment.PotionSlot);
    }

    private void UpdateEquipSlot(VisualElement slot, ItemData item)
    {
        slot.Clear();
        if (item == null)
            return;

        var icon = new VisualElement();
        icon.AddToClassList("equip-slot-icon");
        icon.style.backgroundImage = new StyleBackground(item.Icon);
        // Register hover on the freshly-created icon so callbacks don't accumulate on the slot.
        GameManager.Instance.ItemTooltip.RegisterHover(icon, item);
        slot.Add(icon);

        if (item.Category == ItemCategory.Potion)
        {
            var qty = new Label(_equipment.PotionSlotQuantity.ToString());
            qty.AddToClassList("slot-quantity");
            slot.Add(qty);
        }
    }

    private void RegisterEquipSlotClick(VisualElement slot, ItemCategory category)
    {
        slot.RegisterCallback<ClickEvent>(_ =>
        {
            ItemData equipped = _equipment.GetSlot(category);
            if (equipped == null)
                return;
            GameManager.Instance.ItemTooltip.Hide();
            int qty = category == ItemCategory.Potion ? _equipment.PotionSlotQuantity : 1;
            _inventory.AddItem(equipped, qty);
            _equipment.Unequip(category);
        });
    }

    private void EquipItem(ItemData item)
    {
        GameManager.Instance.ItemTooltip.Hide();

        if (item.Category == ItemCategory.Potion)
        {
            ItemData currentPotion = _equipment.PotionSlot;
            if (currentPotion != null && currentPotion != item)
                _inventory.AddItem(currentPotion, _equipment.PotionSlotQuantity);

            int qty = _inventory.GetQuantity(item);
            if (qty <= 0)
                return;
            _inventory.RemoveItem(item, qty);
            _equipment.EquipPotion(item, qty);
            return;
        }

        ItemData currentlyEquipped = _equipment.GetSlot(item.Category);
        if (currentlyEquipped != null)
            _inventory.AddItem(currentlyEquipped, 1);
        _inventory.RemoveItem(item, 1);
        _equipment.Equip(item);
    }

    private static string FormatQuantity(int qty)
    {
        if (qty >= 1_000_000) return $"{qty / 1_000_000f:0.#}M";
        if (qty >= 1_000) return $"{qty / 1_000f:0.#}K";
        return qty.ToString();
    }

    #endregion
}
