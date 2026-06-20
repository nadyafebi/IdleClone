using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType { Strength = 0, Resilience = 1, Vitality = 2, Yield = 3 }

[Serializable]
public struct UpgradeTierEntry
{
    public IngredientEntry[] Costs;

    [TextArea(1, 2)]
    public string EffectDescription;
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "IdleClone/Upgrade")]
public class UpgradeData : ScriptableObject
{
    #region Serialized Fields

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private UpgradeTierEntry[] _tiers;

    #endregion

    #region Public Properties

    public string DisplayName => _displayName;
    public IReadOnlyList<UpgradeTierEntry> Tiers => _tiers;
    public int MaxTier => _tiers?.Length ?? 0;

    #endregion
}
