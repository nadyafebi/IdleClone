using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    #region Public Properties

    public int CurrentHealth { get; private set; }
    public int MaxHealth => _maxHealth;

    public event Action OnDied;
    public event Action OnTookDamage;
    public event Action<int, int> OnHealthChanged; // (current, max)

    #endregion

    #region Private Fields

    private int _maxHealth = 10;
    private bool _isDead;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        CurrentHealth = _maxHealth;
    }

    #endregion

    #region Public Methods

    public void SetMaxHealth(int maxHealth)
    {
        _maxHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (_isDead)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnTookDamage?.Invoke();
        OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

        if (CurrentHealth <= 0)
            Die();
    }

    public void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        CurrentHealth = 0;
        OnDied?.Invoke();
    }

    public void ResetHealth()
    {
        _isDead = false;
        CurrentHealth = _maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
    }

    #endregion
}
