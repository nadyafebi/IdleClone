using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private PlayerStatsData _data;

    [SerializeField]
    private PlayerLevel _playerLevel;

    [SerializeField]
    private PlayerEquipment _playerEquipment;

    [SerializeField]
    private PlayerUpgrades _playerUpgrades;

    #endregion

    #region Public Properties

    public int MaxHealth => Mathf.RoundToInt(
        (_data.BaseMaxHealth + (_playerLevel.Level - 1) * _data.HealthPerLevel)
        * (1f + VitalityTier * 0.1f));

    public int TotalAttack => Mathf.RoundToInt(
        (_data.BaseAttackDamage + _playerEquipment.TotalAttackBonus)
        * (1f + StrengthTier * 0.1f));

    public int TotalDefense => Mathf.RoundToInt(
        _playerEquipment.TotalDefenseBonus
        * (1f + ResilienceTier * 0.1f));

    public float AttackCooldown => _data.AttackCooldown;
    public float AttackRange => _data.AttackRange;

    public int GatheringDamage => _data.BaseGatheringDamage + Mathf.Min(YieldTier, 4);
    public float GatherCooldown => _data.GatherCooldown * (YieldTier >= 5 ? 0.8f : 1f);
    public float GatherRange => _data.GatherRange;

    public float WalkSpeed => _data.WalkSpeed;
    public float ClimbSpeed => _data.ClimbSpeed;
    public float JumpArcHeight => _data.JumpArcHeight;

    public PlayerStatsData StatsData => _data;

    public event Action OnMaxHealthChanged;

    #endregion

    #region Private Properties

    private int StrengthTier   => _playerUpgrades != null ? _playerUpgrades.StrengthTier   : 0;
    private int ResilienceTier => _playerUpgrades != null ? _playerUpgrades.ResilienceTier : 0;
    private int VitalityTier   => _playerUpgrades != null ? _playerUpgrades.VitalityTier   : 0;
    private int YieldTier      => _playerUpgrades != null ? _playerUpgrades.YieldTier      : 0;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (_data == null)
        {
            Debug.LogError("[PlayerStats] PlayerStatsData asset is not assigned!");
            enabled = false;
            return;
        }
        if (_playerLevel == null)
        {
            Debug.LogError("[PlayerStats] PlayerLevel reference is not assigned!");
            enabled = false;
            return;
        }
        if (_playerEquipment == null)
        {
            Debug.LogError("[PlayerStats] PlayerEquipment reference is not assigned!");
            enabled = false;
            return;
        }

        _playerLevel.OnLevelChanged += _ => OnMaxHealthChanged?.Invoke();

        if (_playerUpgrades != null)
            _playerUpgrades.OnUpgraded += () => OnMaxHealthChanged?.Invoke();
    }

    #endregion
}
