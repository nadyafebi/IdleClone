using System;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private PlayerStatsData _data;

    #endregion

    #region Public Properties

    public int Level { get; private set; } = 1;
    public int CurrentXp { get; private set; }
    public int XpToNextLevel => Mathf.FloorToInt(_data.BaseXp * Mathf.Pow(Level, _data.XpExponent));

    public event Action<int> OnLevelChanged;   // new level
    public event Action<int, int> OnXpChanged; // (currentXp, xpToNextLevel)

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (_data == null)
        {
            Debug.LogError("[PlayerLevel] PlayerStatsData asset is not assigned!");
            enabled = false;
        }
    }

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

    public void LoadLevel(int level, int xp)
    {
        Level     = level;
        CurrentXp = xp;
        OnLevelChanged?.Invoke(Level);
        OnXpChanged?.Invoke(CurrentXp, XpToNextLevel);
    }

    #endregion
}
