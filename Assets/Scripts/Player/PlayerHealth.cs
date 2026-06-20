using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    [Min(1)]
    private int _maxHealth = 100;

    #endregion

    #region Public Properties

    public int CurrentHealth { get; private set; }
    public int MaxHealth => _maxHealth;

    public event Action OnDied;
    public event Action<int, int> OnHealthChanged; // (current, max)

    #endregion

    #region Private Fields

    private bool _isDead;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        CurrentHealth = _maxHealth;
    }

    #endregion

    #region Public Methods

    public int Heal(int amount)
    {
        if (_isDead || amount <= 0)
            return 0;
        int actual = Mathf.Min(amount, _maxHealth - CurrentHealth);
        if (actual <= 0)
            return 0;
        CurrentHealth += actual;
        OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        return actual;
    }

    public void TakeDamage(int amount)
    {
        if (_isDead || amount <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

        if (CurrentHealth <= 0)
            Die();
    }

    public void ResetHealth()
    {
        _isDead = false;
        CurrentHealth = _maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
    }

    #endregion

    #region Private Methods

    private void Die()
    {
        _isDead = true;
        OnDied?.Invoke();
    }

    #endregion
}
