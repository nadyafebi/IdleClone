using System.Collections.Generic;
using UnityEngine.UIElements;

public class ShopMenu : GameMenu
{
    #region Private Fields

    private VisualElement _panel;
    private ScrollView _shopList;
    private Label _titleLabel;
    private PlayerInventory _inventory;
    private ItemData _currency;
    private readonly List<ShopItemEntry> _entries = new();
    private readonly List<(ShopItemEntry Entry, Button Button)> _shopButtons = new();

    #endregion

    #region Protected Properties

    protected override VisualElement BlockingElement => _panel;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _inventory = GameManager.Instance.PlayerInventory;
        _panel = Root.Q("shop-panel");
        _shopList = Root.Q<ScrollView>("shop-list");
        _titleLabel = Root.Q<Label>("shop-label");
    }

    #endregion

    #region Public Methods

    public void Open(string shopName, ItemData currency, IReadOnlyList<ShopItemEntry> entries)
    {
        _currency = currency;
        _entries.Clear();
        _entries.AddRange(entries);

        if (_titleLabel != null)
            _titleLabel.text = shopName;

        BuildRows();

        if (!IsVisible)
            Show();
        else
            RefreshButtons();
    }

    #endregion

    #region Protected Methods

    protected override void OnShow()
    {
        _inventory.OnChanged += RefreshButtons;
        RefreshButtons();
    }

    protected override void OnHide()
    {
        _inventory.OnChanged -= RefreshButtons;
        GameManager.Instance.ItemTooltip.Hide();
    }

    #endregion

    #region Private Helpers

    private void BuildRows()
    {
        _shopList.Clear();
        _shopButtons.Clear();

        VisualElement lastRow = null;

        foreach (ShopItemEntry entry in _entries)
        {
            if (entry.Item == null)
                continue;

            var row = new VisualElement();
            row.AddToClassList("shop-row");
            lastRow = row;

            // Item icon
            var itemWrapper = new VisualElement();
            itemWrapper.AddToClassList("shop-item-wrapper");

            var itemIcon = new VisualElement();
            itemIcon.AddToClassList("shop-item-icon");
            itemIcon.style.backgroundImage = new StyleBackground(entry.Item.Icon);
            itemWrapper.Add(itemIcon);
            GameManager.Instance.ItemTooltip.RegisterHover(itemWrapper, entry.Item);
            row.Add(itemWrapper);

            // Info column: name + price chip stacked
            var infoColumn = new VisualElement();
            infoColumn.AddToClassList("shop-item-info");

            var nameLabel = new Label(entry.Item.DisplayName);
            nameLabel.AddToClassList("shop-item-name");
            infoColumn.Add(nameLabel);

            // Price chip (inside info column)
            var priceChip = new VisualElement();
            priceChip.AddToClassList("shop-price-chip");

            if (_currency != null)
            {
                var currencyIcon = new VisualElement();
                currencyIcon.AddToClassList("shop-currency-icon");
                currencyIcon.style.backgroundImage = new StyleBackground(_currency.Icon);
                priceChip.Add(currencyIcon);
            }

            var priceLabel = new Label(entry.Price.ToString());
            priceLabel.AddToClassList("shop-price-label");
            priceChip.Add(priceLabel);
            infoColumn.Add(priceChip);
            row.Add(infoColumn);

            // Buy button
            ShopItemEntry captured = entry;
            var buyButton = new Button(() => TryBuy(captured));
            buyButton.text = "Buy";
            buyButton.AddToClassList("buy-button");
            row.Add(buyButton);

            _shopButtons.Add((entry, buyButton));
            _shopList.Add(row);
        }

        lastRow?.AddToClassList("shop-row--last");
    }

    private void RefreshButtons()
    {
        foreach ((ShopItemEntry entry, Button button) in _shopButtons)
            button.SetEnabled(CanAfford(entry));
    }

    private bool CanAfford(ShopItemEntry entry)
    {
        if (_currency == null)
            return false;
        return _inventory.Items.TryGetValue(_currency, out int qty) && qty >= entry.Price;
    }

    private void TryBuy(ShopItemEntry entry)
    {
        if (!CanAfford(entry))
            return;

        _inventory.RemoveItem(_currency, entry.Price);
        _inventory.AddItem(entry.Item, 1);
    }

    #endregion
}
