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
    private int _activeTab;

    #endregion

    #region Protected Properties

    // Narrows blocking to the visible panel, not the full-screen document root.
    protected override VisualElement BlockingElement => _panel;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _inventory = GameManager.Instance.PlayerInventory;
        _panel = Root.Q("inventory-panel");
        _tabBar = Root.Q("tab-bar");
        _grid = Root.Q("inventory-grid");
        BuildTabs();
    }

    #endregion

    #region Protected Methods

    protected override void OnShow()
    {
        _inventory.OnChanged += Rebuild;
        Rebuild();
    }

    protected override void OnHide()
    {
        _inventory.OnChanged -= Rebuild;
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
                var icon = new VisualElement();
                icon.AddToClassList("slot-icon");
                icon.style.backgroundImage = new StyleBackground(items[itemIndex].Key.Icon);

                var label = new Label(FormatQuantity(items[itemIndex].Value));
                label.AddToClassList("slot-quantity");

                slot.Add(icon);
                slot.Add(label);
            }

            _grid.Add(slot);
        }
    }

    private static string FormatQuantity(int qty)
    {
        if (qty >= 1_000_000) return $"{qty / 1_000_000f:0.#}M";
        if (qty >= 1_000) return $"{qty / 1_000f:0.#}K";
        return qty.ToString();
    }

    #endregion
}
