using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStatsMenu : GameMenu
{
    #region Private Fields

    private VisualElement _panel;
    private PlayerStats _stats;
    private PlayerLevel _playerLevel;
    private PlayerEquipment _equipment;
    private PlayerUpgrades _upgrades;

    private Label _levelLabel;
    private Label _hpLabel;
    private Label _attackLabel;
    private Label _defenseLabel;
    private Label _atkSpeedLabel;
    private Label _gatherDmgLabel;
    private Label _gatherSpdLabel;

    #endregion

    #region Protected Properties

    protected override VisualElement BlockingElement => _panel;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _stats = GameManager.Instance.PlayerStats;
        _playerLevel = GameManager.Instance.PlayerLevel;
        _equipment = GameManager.Instance.PlayerEquipment;
        _upgrades = GameManager.Instance.PlayerUpgrades;

        _panel = Root.Q("player-stats-panel");
        _levelLabel = Root.Q<Label>("stat-level");
        _hpLabel = Root.Q<Label>("stat-hp");
        _attackLabel = Root.Q<Label>("stat-attack");
        _defenseLabel = Root.Q<Label>("stat-defense");
        _atkSpeedLabel = Root.Q<Label>("stat-atk-speed");
        _gatherDmgLabel = Root.Q<Label>("stat-gather-dmg");
        _gatherSpdLabel = Root.Q<Label>("stat-gather-spd");

        if (_stats == null || _playerLevel == null || _equipment == null)
        {
            Debug.LogError("[PlayerStatsMenu] Missing required GameManager components.");
            enabled = false;
        }
    }

    #endregion

    #region Protected Methods

    protected override void OnShow()
    {
        _playerLevel.OnLevelChanged += HandleLevelChanged;
        _equipment.OnEquipmentChanged += RefreshStats;
        _stats.OnMaxHealthChanged += RefreshStats;
        if (_upgrades != null)
            _upgrades.OnUpgraded += RefreshStats;
        RefreshStats();
    }

    protected override void OnHide()
    {
        _playerLevel.OnLevelChanged -= HandleLevelChanged;
        _equipment.OnEquipmentChanged -= RefreshStats;
        _stats.OnMaxHealthChanged -= RefreshStats;
        if (_upgrades != null)
            _upgrades.OnUpgraded -= RefreshStats;
    }

    #endregion

    #region Private Helpers

    private void HandleLevelChanged(int _) => RefreshStats();

    private void RefreshStats()
    {
        if (_levelLabel != null)    _levelLabel.text    = _playerLevel.Level.ToString();
        if (_hpLabel != null)       _hpLabel.text       = _stats.MaxHealth.ToString();
        if (_attackLabel != null)   _attackLabel.text   = _stats.TotalAttack.ToString();
        if (_defenseLabel != null)  _defenseLabel.text  = _stats.TotalDefense.ToString();
        if (_atkSpeedLabel != null) _atkSpeedLabel.text = $"{_stats.AttackCooldown:0.0}s";
        if (_gatherDmgLabel != null) _gatherDmgLabel.text = _stats.GatheringDamage.ToString();
        if (_gatherSpdLabel != null) _gatherSpdLabel.text = $"{_stats.GatherCooldown:0.0}s";
    }

    #endregion
}
