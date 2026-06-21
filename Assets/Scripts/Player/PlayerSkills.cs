using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    #region Serialized Fields

    [Header("Prefabs")]
    [SerializeField]
    private GameObject _slashEffectPrefab;

    [SerializeField]
    private GameObject _barrierEffectPrefab;

    [SerializeField]
    private GameObject _fireballPrefab;

    [Header("Icons")]
    [SerializeField]
    private Sprite _slashIcon;

    [SerializeField]
    private Sprite _barrierIcon;

    [SerializeField]
    private Sprite _fireballIcon;

    [Header("Cooldowns")]
    [SerializeField]
    private float _slashCooldown = 8f;

    [SerializeField]
    private float _barrierCooldown = 25f;

    [SerializeField]
    private float _fireballCooldown = 10f;

    [Header("Slash AoE")]
    [Tooltip("How far ahead of the player the AoE circle is centered.")]
    [SerializeField]
    private float _slashAoeOffset = 2.5f;

    [SerializeField]
    private float _slashAoeRadius = 2.5f;

    #endregion

    #region Public Properties

    public bool SlashUnlocked => _playerEquipment != null && _playerEquipment.WeaponSlot != null;
    public bool BarrierUnlocked => _playerEquipment != null && _playerEquipment.ShieldSlot != null;
    public bool FireballUnlocked { get; private set; }

    public bool SlashReady => _slashCooldownRemaining <= 0f;
    public bool BarrierReady => _barrierCooldownRemaining <= 0f;
    public bool FireballReady => _fireballCooldownRemaining <= 0f;

    public float SlashCooldownRemaining => _slashCooldownRemaining;
    public float BarrierCooldownRemaining => _barrierCooldownRemaining;
    public float FireballCooldownRemaining => _fireballCooldownRemaining;

    public Sprite SlashIcon => _slashIcon;
    public Sprite BarrierIcon => _barrierIcon;
    public Sprite FireballIcon => _fireballIcon;

    public event Action OnSkillStateChanged;

    #endregion

    #region Private Fields

    private PlayerEquipment _playerEquipment;
    private PlayerStats _playerStats;

    private float _slashCooldownRemaining;
    private float _barrierCooldownRemaining;
    private float _fireballCooldownRemaining;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _playerEquipment = GameManager.Instance?.PlayerEquipment;
        _playerStats = GameManager.Instance?.PlayerStats;

        if (_playerEquipment == null)
        {
            Debug.LogError("[PlayerSkills] PlayerEquipment not found on GameManager.");
            enabled = false;
            return;
        }

        _playerEquipment.OnEquipmentChanged += HandleEquipmentChanged;
        OnSkillStateChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (_playerEquipment != null)
            _playerEquipment.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    private void Update()
    {
        bool anyJustReady = false;

        if (_slashCooldownRemaining > 0f)
        {
            _slashCooldownRemaining -= Time.deltaTime;
            if (_slashCooldownRemaining <= 0f)
            {
                _slashCooldownRemaining = 0f;
                anyJustReady = true;
            }
        }

        if (_barrierCooldownRemaining > 0f)
        {
            _barrierCooldownRemaining -= Time.deltaTime;
            if (_barrierCooldownRemaining <= 0f)
            {
                _barrierCooldownRemaining = 0f;
                anyJustReady = true;
            }
        }

        if (_fireballCooldownRemaining > 0f)
        {
            _fireballCooldownRemaining -= Time.deltaTime;
            if (_fireballCooldownRemaining <= 0f)
            {
                _fireballCooldownRemaining = 0f;
                anyJustReady = true;
            }
        }

        if (anyJustReady)
            OnSkillStateChanged?.Invoke();
    }

    #endregion

    #region Public Methods

    public void TryCastSlash()
    {
        if (!SlashUnlocked || !SlashReady)
            return;

        _slashCooldownRemaining = _slashCooldown;

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player == null)
        {
            OnSkillStateChanged?.Invoke();
            return;
        }

        Vector2 facing = player.FacingRight ? Vector2.right : Vector2.left;
        float damage = (_playerStats != null ? _playerStats.TotalAttack : 1) * 2f;

        ApplySlashAoeDamage((Vector2)player.transform.position, facing, damage);

        if (_slashEffectPrefab != null)
        {
            GameObject go = Instantiate(_slashEffectPrefab);
            go.GetComponent<SlashEffect>()
                ?.Init((Vector2)player.transform.position + facing * 0.4f, facing.x > 0f);
        }

        OnSkillStateChanged?.Invoke();
    }

    public void TryCastBarrier()
    {
        if (!BarrierUnlocked || !BarrierReady)
            return;

        _barrierCooldownRemaining = _barrierCooldown;

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        PlayerHealth health = GameManager.Instance?.PlayerHealth;

        if (player != null && health != null && _barrierEffectPrefab != null)
        {
            GameObject go = Instantiate(_barrierEffectPrefab);
            go.GetComponent<BarrierEffect>()?.Init(player.transform, health);
        }

        OnSkillStateChanged?.Invoke();
    }

    public void TryCastFireball()
    {
        if (!FireballUnlocked || !FireballReady)
            return;

        _fireballCooldownRemaining = _fireballCooldown;

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player == null)
        {
            OnSkillStateChanged?.Invoke();
            return;
        }

        Vector2 facing = player.FacingRight ? Vector2.right : Vector2.left;
        float damage = (_playerStats != null ? _playerStats.TotalAttack : 1) * 3f;
        Vector2 spawnPos = (Vector2)player.transform.position + facing * 0.8f + new Vector2(0, .5f);

        if (_fireballPrefab != null)
        {
            GameObject go = Instantiate(_fireballPrefab, spawnPos, Quaternion.identity);
            go.GetComponent<FireballProjectile>()?.Init(facing, damage);
        }

        OnSkillStateChanged?.Invoke();
    }

    public void UnlockFireball()
    {
        if (FireballUnlocked)
            return;

        FireballUnlocked = true;
        Debug.Log("[PlayerSkills] Fireball unlocked.");
        OnSkillStateChanged?.Invoke();
    }

    public void LoadFireball(bool unlocked)
    {
        FireballUnlocked = unlocked;
        OnSkillStateChanged?.Invoke();
    }

    #endregion

    #region Private Methods

    private void ApplySlashAoeDamage(Vector2 origin, Vector2 facing, float damage)
    {
        Vector2 center = origin + facing * _slashAoeOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, _slashAoeRadius);

        var damaged = new HashSet<EnemyHealth>();
        foreach (Collider2D col in hits)
        {
            EnemyHealth enemy =
                col.GetComponent<EnemyHealth>() ?? col.GetComponentInParent<EnemyHealth>();

            if (enemy == null || damaged.Contains(enemy))
                continue;

            if (Vector2.Dot((Vector2)enemy.transform.position - origin, facing) < 0f)
                continue;

            damaged.Add(enemy);
            enemy.TakeDamage(Mathf.RoundToInt(damage));
        }
    }

    private void HandleEquipmentChanged() => OnSkillStateChanged?.Invoke();

    #endregion
}
