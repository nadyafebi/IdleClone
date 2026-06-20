using System;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UpgradeData _strengthData;

    [SerializeField]
    private UpgradeData _resilienceData;

    [SerializeField]
    private UpgradeData _vitalityData;

    [SerializeField]
    private UpgradeData _yieldData;

    #endregion

    #region Public Properties

    // These 4 ints are the complete save/load surface for upgrades.
    public int StrengthTier { get; private set; }
    public int ResilienceTier { get; private set; }
    public int VitalityTier { get; private set; }
    public int YieldTier { get; private set; }

    public event Action OnUpgraded;

    #endregion

    #region Public Methods

    public int GetTier(UpgradeType type) => type switch
    {
        UpgradeType.Strength   => StrengthTier,
        UpgradeType.Resilience => ResilienceTier,
        UpgradeType.Vitality   => VitalityTier,
        UpgradeType.Yield      => YieldTier,
        _                      => 0
    };

    public UpgradeData GetData(UpgradeType type) => type switch
    {
        UpgradeType.Strength   => _strengthData,
        UpgradeType.Resilience => _resilienceData,
        UpgradeType.Vitality   => _vitalityData,
        UpgradeType.Yield      => _yieldData,
        _                      => null
    };

    public bool CanAfford(UpgradeType type, PlayerInventory inventory)
    {
        UpgradeData data = GetData(type);
        if (data == null) return false;

        int currentTier = GetTier(type);
        if (currentTier >= data.MaxTier) return false;

        return CanAffordTier(data.Tiers[currentTier], inventory);
    }

    public bool TryUpgrade(UpgradeType type, PlayerInventory inventory)
    {
        UpgradeData data = GetData(type);
        if (data == null) return false;

        int currentTier = GetTier(type);
        if (currentTier >= data.MaxTier) return false;

        UpgradeTierEntry nextTier = data.Tiers[currentTier];
        if (!CanAffordTier(nextTier, inventory)) return false;

        foreach (IngredientEntry cost in nextTier.Costs)
        {
            if (cost.Item != null)
                inventory.RemoveItem(cost.Item, cost.Quantity);
        }

        IncrementTier(type);
        OnUpgraded?.Invoke();
        return true;
    }

    // For save/load: restore all tiers at once.
    public void LoadTiers(int strength, int resilience, int vitality, int yield)
    {
        StrengthTier   = strength;
        ResilienceTier = resilience;
        VitalityTier   = vitality;
        YieldTier      = yield;
        OnUpgraded?.Invoke();
    }

    #endregion

    #region Private Helpers

    private bool CanAffordTier(UpgradeTierEntry tier, PlayerInventory inventory)
    {
        foreach (IngredientEntry cost in tier.Costs)
        {
            if (cost.Item == null) continue;
            if (!inventory.Items.TryGetValue(cost.Item, out int qty) || qty < cost.Quantity)
                return false;
        }
        return true;
    }

    private void IncrementTier(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Strength:   StrengthTier++;   break;
            case UpgradeType.Resilience: ResilienceTier++; break;
            case UpgradeType.Vitality:   VitalityTier++;   break;
            case UpgradeType.Yield:      YieldTier++;      break;
        }
    }

    #endregion
}
