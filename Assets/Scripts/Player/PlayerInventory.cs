using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    #region Public Properties

    public IReadOnlyDictionary<ItemData, int> Items => _items;

    public event Action OnChanged;

    #endregion

    #region Private Fields

    private readonly Dictionary<ItemData, int> _items = new();

    #endregion

    #region Public Methods

    public void AddItem(ItemData item, int quantity)
    {
        _items.TryGetValue(item, out int current);
        _items[item] = current + quantity;
        OnChanged?.Invoke();
        Debug.Log($"[PlayerInventory] +{quantity} {item.DisplayName} (total: {_items[item]})");
    }

    #endregion
}
