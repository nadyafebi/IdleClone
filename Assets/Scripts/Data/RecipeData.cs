using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct IngredientEntry
{
    public ItemData Item;
    public int Quantity;
}

[CreateAssetMenu(fileName = "NewRecipe", menuName = "IdleClone/Recipe")]
public class RecipeData : ScriptableObject
{
    #region Serialized Fields

    [SerializeField]
    private IngredientEntry[] _ingredients;

    [SerializeField]
    private ItemData _resultItem;

    [SerializeField]
    private int _resultQuantity = 1;

    #endregion

    #region Public Properties

    public IReadOnlyList<IngredientEntry> Ingredients => _ingredients;
    public ItemData ResultItem => _resultItem;
    public int ResultQuantity => _resultQuantity;

    #endregion
}
