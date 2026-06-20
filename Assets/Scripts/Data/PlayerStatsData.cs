using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "IdleClone/Player Stats Data")]
public class PlayerStatsData : ScriptableObject
{
    #region Serialized Fields

    [Header("Health")]
    [SerializeField]
    [Min(1)]
    private int _baseMaxHealth = 100;

    [SerializeField]
    [Min(0)]
    private int _healthPerLevel = 15;

    [SerializeField]
    [Min(1)]
    private int _maxLevel = 12;

    [Header("Combat")]
    [SerializeField]
    [Min(1)]
    private int _baseAttackDamage = 1;

    [SerializeField]
    [Min(0.1f)]
    private float _attackCooldown = 1f;

    [SerializeField]
    [Min(0.1f)]
    private float _attackRange = 2f;

    [Header("Gathering")]
    [SerializeField]
    [Min(1)]
    private int _baseGatheringDamage = 1;

    [SerializeField]
    [Min(0.1f)]
    private float _gatherCooldown = 1f;

    [SerializeField]
    [Min(0.1f)]
    private float _gatherRange = 2f;

    [Header("Movement")]
    [SerializeField]
    [Min(0.1f)]
    private float _walkSpeed = 4f;

    [SerializeField]
    [Min(0.1f)]
    private float _climbSpeed = 3f;

    [SerializeField]
    [Min(0.1f)]
    private float _jumpArcHeight = 2f;

    [Header("Leveling")]
    [SerializeField]
    [Min(1)]
    private int _baseXp = 100;

    [SerializeField]
    [Min(0.1f)]
    private float _xpExponent = 1.5f;

    #endregion

    #region Public Properties

    public int BaseMaxHealth => _baseMaxHealth;
    public int HealthPerLevel => _healthPerLevel;
    public int MaxLevel => _maxLevel;

    public int BaseAttackDamage => _baseAttackDamage;
    public float AttackCooldown => _attackCooldown;
    public float AttackRange => _attackRange;

    public int BaseGatheringDamage => _baseGatheringDamage;
    public float GatherCooldown => _gatherCooldown;
    public float GatherRange => _gatherRange;

    public float WalkSpeed => _walkSpeed;
    public float ClimbSpeed => _climbSpeed;
    public float JumpArcHeight => _jumpArcHeight;

    public int BaseXp => _baseXp;
    public float XpExponent => _xpExponent;

    #endregion
}
