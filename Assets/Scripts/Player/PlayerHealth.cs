using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private PlayerStats _stats;

    #endregion

    #region Public Properties

    public int CurrentHealth { get; private set; }
    public int MaxHealth => _stats.MaxHealth;
    public bool IsInvincible { get; set; }

    public event Action OnDied;
    public event Action<int, int> OnHealthChanged; // (current, max)

    #endregion

    #region Private Fields

    private bool _isDead;
    private int _previousMaxHealth;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _previousMaxHealth = _stats.MaxHealth;
        CurrentHealth = _previousMaxHealth;
    }

    private void Start()
    {
        _stats.OnMaxHealthChanged += HandleMaxHealthChanged;
    }

    private void OnDestroy()
    {
        _stats.OnMaxHealthChanged -= HandleMaxHealthChanged;
    }

    #endregion

    #region Public Methods

    public int Heal(int amount)
    {
        if (_isDead || amount <= 0)
            return 0;
        int actual = Mathf.Min(amount, _stats.MaxHealth - CurrentHealth);
        if (actual <= 0)
            return 0;
        CurrentHealth += actual;
        OnHealthChanged?.Invoke(CurrentHealth, _stats.MaxHealth);
        return actual;
    }

    public void TakeDamage(int amount)
    {
        if (_isDead || IsInvincible || amount <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, _stats.MaxHealth);

        if (CurrentHealth <= 0)
            Die();
    }

    public void ResetHealth()
    {
        _isDead = false;
        _previousMaxHealth = _stats.MaxHealth;
        CurrentHealth = _previousMaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, _stats.MaxHealth);
    }

    #endregion

    #region Private Methods

    private void HandleMaxHealthChanged()
    {
        int gain = _stats.MaxHealth - _previousMaxHealth;
        _previousMaxHealth = _stats.MaxHealth;
        if (gain > 0)
            CurrentHealth = Mathf.Min(CurrentHealth + gain, _stats.MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, _stats.MaxHealth);
    }

    private void Die()
    {
        _isDead = true;
        OnDied?.Invoke();
    }

    #endregion
}
