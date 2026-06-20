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

    #endregion

    #region Public Properties

    public int MaxHealth => _data.BaseMaxHealth + (_playerLevel.Level - 1) * _data.HealthPerLevel;

    public int TotalAttack => _data.BaseAttackDamage + _playerEquipment.TotalAttackBonus;
    public int TotalDefense => _playerEquipment.TotalDefenseBonus;
    public float AttackCooldown => _data.AttackCooldown;
    public float AttackRange => _data.AttackRange;

    public int GatheringDamage => _data.BaseGatheringDamage;
    public float GatherCooldown => _data.GatherCooldown;
    public float GatherRange => _data.GatherRange;

    public float WalkSpeed => _data.WalkSpeed;
    public float ClimbSpeed => _data.ClimbSpeed;
    public float JumpArcHeight => _data.JumpArcHeight;

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
        }
    }

    #endregion
}
