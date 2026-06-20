using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UpgradeMenu : GameMenu
{
    #region Private Fields

    private VisualElement _panel;
    private ScrollView _upgradeList;
    private PlayerInventory _inventory;
    private PlayerUpgrades _upgrades;

    private readonly List<(UpgradeType Type, Button Button)> _upgradeButtons = new();

    #endregion

    #region Protected Properties

    protected override VisualElement BlockingElement => _panel;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _inventory = GameManager.Instance.PlayerInventory;
        _upgrades = GameManager.Instance.PlayerUpgrades;

        if (_upgrades == null)
        {
            Debug.LogError("[UpgradeMenu] No PlayerUpgrades found on GameManager.");
            enabled = false;
            return;
        }

        _panel = Root.Q("upgrade-panel");
        _upgradeList = Root.Q<ScrollView>("upgrade-list");
        BuildList();
    }

    #endregion

    #region Protected Methods

    protected override void OnShow()
    {
        _inventory.OnChanged += RefreshButtons;
        _upgrades.OnUpgraded += RebuildList;
        RefreshButtons();
    }

    protected override void OnHide()
    {
        _inventory.OnChanged -= RefreshButtons;
        _upgrades.OnUpgraded -= RebuildList;
        GameManager.Instance.ItemTooltip.Hide();
    }

    #endregion

    #region Private Helpers

    private void BuildList()
    {
        _upgradeList.Clear();
        _upgradeButtons.Clear();

        VisualElement lastRow = null;

        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            UpgradeData data = _upgrades.GetData(type);
            if (data == null)
                continue;

            VisualElement row = BuildRow(type, data);
            _upgradeList.Add(row);
            lastRow = row;
        }

        lastRow?.AddToClassList("upgrade-row--last");
    }

    private VisualElement BuildRow(UpgradeType type, UpgradeData data)
    {
        int currentTier = _upgrades.GetTier(type);
        bool maxed = currentTier >= data.MaxTier;

        var row = new VisualElement();
        row.AddToClassList("upgrade-row");

        // Header: name + tier label
        var header = new VisualElement();
        header.AddToClassList("upgrade-row-header");

        var nameLabel = new Label(data.DisplayName);
        nameLabel.AddToClassList("upgrade-name");

        var tierLabel = new Label($"Tier {currentTier} / {data.MaxTier}");
        tierLabel.AddToClassList("upgrade-tier");

        header.Add(nameLabel);
        header.Add(tierLabel);
        row.Add(header);

        // Current and next effect lines
        Label lastEffectLabel = null;

        if (currentTier > 0)
        {
            var currentLabel = new Label($"Current: {data.Tiers[currentTier - 1].EffectDescription}");
            currentLabel.AddToClassList("upgrade-effect");
            currentLabel.AddToClassList("upgrade-effect--current");
            row.Add(currentLabel);
            lastEffectLabel = currentLabel;
        }

        if (!maxed)
        {
            var nextLabel = new Label($"Next: {data.Tiers[currentTier].EffectDescription}");
            nextLabel.AddToClassList("upgrade-effect");
            nextLabel.AddToClassList("upgrade-effect--next");
            row.Add(nextLabel);
            lastEffectLabel = nextLabel;
        }

        lastEffectLabel?.AddToClassList("upgrade-effect--last");

        // Cost chips + Upgrade button
        var costsRow = new VisualElement();
        costsRow.AddToClassList("upgrade-costs-row");

        var chips = new VisualElement();
        chips.AddToClassList("upgrade-chips");

        if (!maxed)
        {
            foreach (IngredientEntry cost in data.Tiers[currentTier].Costs)
            {
                if (cost.Item == null)
                    continue;

                var chip = new VisualElement();
                chip.AddToClassList("ingredient-chip");

                var icon = new VisualElement();
                icon.AddToClassList("ingredient-icon");
                icon.style.backgroundImage = new StyleBackground(cost.Item.Icon);

                var countLabel = new Label($"x{cost.Quantity}");
                countLabel.AddToClassList("ingredient-label");

                chip.Add(icon);
                chip.Add(countLabel);
                GameManager.Instance.ItemTooltip.RegisterHover(chip, cost.Item);
                chips.Add(chip);
            }
        }

        costsRow.Add(chips);

        UpgradeType captured = type;
        var upgradeBtn = new Button(() => TryUpgrade(captured));
        upgradeBtn.text = maxed ? "MAX" : "Upgrade";
        upgradeBtn.AddToClassList("craft-button");
        upgradeBtn.SetEnabled(!maxed && _upgrades.CanAfford(type, _inventory));

        costsRow.Add(upgradeBtn);
        row.Add(costsRow);

        _upgradeButtons.Add((type, upgradeBtn));
        return row;
    }

    private void RebuildList()
    {
        BuildList();
    }

    private void RefreshButtons()
    {
        foreach ((UpgradeType type, Button button) in _upgradeButtons)
        {
            UpgradeData data = _upgrades.GetData(type);
            bool maxed = data == null || _upgrades.GetTier(type) >= data.MaxTier;
            button.SetEnabled(!maxed && _upgrades.CanAfford(type, _inventory));
        }
    }

    private void TryUpgrade(UpgradeType type)
    {
        _upgrades.TryUpgrade(type, _inventory);
    }

    #endregion
}
