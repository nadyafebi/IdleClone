using System;
using UnityEngine;

public enum PlayerClass { Beginner, Awakened }

public class PlayerProgression : MonoBehaviour
{
    #region Public Properties

    public PlayerClass CurrentClass { get; private set; }

    public event Action<PlayerClass> OnClassChanged;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        CurrentClass = (PlayerClass)PlayerPrefs.GetInt("PlayerClass", (int)PlayerClass.Beginner);
    }

    #endregion

    #region Public Methods

    public void ChangeClass(PlayerClass newClass)
    {
        if (CurrentClass == newClass)
            return;

        CurrentClass = newClass;
        PlayerPrefs.SetInt("PlayerClass", (int)newClass);
        Debug.Log($"[PlayerProgression] Class changed to {newClass}");
        OnClassChanged?.Invoke(newClass);
    }

    // Silent restore for save system — sets state without side effects.
    public void LoadClass(PlayerClass savedClass)
    {
        CurrentClass = savedClass;
        OnClassChanged?.Invoke(savedClass);
    }

    #endregion
}
