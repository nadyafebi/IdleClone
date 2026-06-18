using System;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    #region Serialized Fields

    [Header("XP Scaling")]
    [Tooltip("Base XP required to reach level 2. Formula: floor(baseXp * level ^ exponent)")]
    [SerializeField]
    [Min(1)]
    private int _baseXp = 100;

    [Tooltip("Growth exponent. 1 = linear, 1.5 = steep, 2 = very steep.")]
    [SerializeField]
    [Min(0.1f)]
    private float _exponent = 1.5f;

    #endregion

    #region Public Properties

    public int Level { get; private set; } = 1;
    public int CurrentXp { get; private set; }
    public int XpToNextLevel => Mathf.FloorToInt(_baseXp * Mathf.Pow(Level, _exponent));

    public event Action<int> OnLevelChanged;    // new level
    public event Action<int, int> OnXpChanged;  // (currentXp, xpToNextLevel)

    #endregion

    #region Public Methods

    public void AddXp(int amount)
    {
        if (amount <= 0)
            return;

        CurrentXp += amount;

        while (CurrentXp >= XpToNextLevel)
        {
            CurrentXp -= XpToNextLevel;
            Level++;
            OnLevelChanged?.Invoke(Level);
        }

        OnXpChanged?.Invoke(CurrentXp, XpToNextLevel);
    }

    #endregion
}
