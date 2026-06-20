using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryMenu : GameMenu
{
    #region Serialized Fields

    [SerializeField]
    private int _tabCount = 1;

    #endregion

    #region Private Fields

    private const int SlotsPerTab = 16;

    private VisualElement _panel;
    private VisualElement _tabBar;
    private VisualElement _grid;
    private PlayerInventory _inventory;
    private PlayerEquipment _equipment;
    private int _activeTab;

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
        _tabBar = Root.Q("tab-bar");
        _grid = Root.Q("inventory-grid");

        _equipmentPanel = Root.Q("equipment-panel");
        _slotWeapon = Root.Q("slot-weapon");
        _slotShield = Root.Q("slot-shield");
        _slotPotion = Root.Q("slot-potion");

        RegisterEquipSlotClick(_slotWeapon, ItemCategory.Weapon);
        RegisterEquipSlotClick(_slotShield, ItemCategory.Shield);
        RegisterEquipSlotClick(_slotPotion, ItemCategory.Potion);

        BuildTabs();
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

    private void BuildTabs()
    {
        _tabBar.Clear();
        for (int i = 0; i < _tabCount; i++)
        {
            int tabIndex = i;
            var btn = new Button(() => SelectTab(tabIndex));
            btn.AddToClassList("tab-button");
            btn.text = (i + 1).ToString();
            btn.EnableInClassList("tab-button--active", i == _activeTab);
            _tabBar.Add(btn);
        }
    }

    private void SelectTab(int index)
    {
        _activeTab = index;
        int i = 0;
        foreach (VisualElement child in _tabBar.Children())
            child.EnableInClassList("tab-button--active", i++ == _activeTab);
        Rebuild();
    }

    private void Rebuild()
    {
        _grid.Clear();
        var items = new List<KeyValuePair<ItemData, int>>(_inventory.Items);
        int startIndex = _activeTab * SlotsPerTab;

        for (int i = 0; i < SlotsPerTab; i++)
        {
            var slot = new VisualElement();
            slot.AddToClassList("inventory-slot");

            int itemIndex = startIndex + i;
            if (itemIndex < items.Count)
            {
                ItemData item = items[itemIndex].Key;

                var icon = new VisualElement();
                icon.AddToClassList("slot-icon");
                icon.style.backgroundImage = new StyleBackground(item.Icon);

                var label = new Label(FormatQuantity(items[itemIndex].Value));
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
    }

    private void RegisterEquipSlotClick(VisualElement slot, ItemCategory category)
    {
        slot.RegisterCallback<ClickEvent>(_ =>
        {
            ItemData equipped = _equipment.GetSlot(category);
            if (equipped == null)
                return;
            GameManager.Instance.ItemTooltip.Hide();
            _equipment.Unequip(category);
            _inventory.AddItem(equipped, 1);
        });
    }

    private void EquipItem(ItemData item)
    {
        GameManager.Instance.ItemTooltip.Hide();
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
