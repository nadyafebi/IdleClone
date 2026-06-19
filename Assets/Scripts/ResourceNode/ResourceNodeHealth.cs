using System;
using UnityEngine;

public class ResourceNodeHealth : MonoBehaviour
{
    #region Public Properties

    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    public event Action OnDropTriggered;
    public event Action OnTookDamage;
    public event Action<int, int> OnHealthChanged; // (current, max)

    #endregion

    #region Public Methods

    public void SetMaxHealth(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public void TakeHit(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnTookDamage?.Invoke();
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
            TriggerDrop();
    }

    #endregion

    #region Private Methods

    private void TriggerDrop()
    {
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnDropTriggered?.Invoke();
    }

    #endregion
}
