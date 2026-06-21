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
        ItemPickupNotifier.TrySpawn(item.DisplayName, quantity);
    }

    public int GetQuantity(ItemData item)
    {
        _items.TryGetValue(item, out int qty);
        return qty;
    }

    public bool RemoveItem(ItemData item, int quantity)
    {
        if (item == null || !_items.TryGetValue(item, out int current) || current < quantity)
            return false;

        int newCount = current - quantity;
        if (newCount == 0)
            _items.Remove(item);
        else
            _items[item] = newCount;

        OnChanged?.Invoke();
        return true;
    }

    public void LoadItems(List<ItemSaveEntry> entries, SaveRegistry registry)
    {
        _items.Clear();
        foreach (ItemSaveEntry entry in entries)
        {
            if (entry.quantity <= 0)
                continue;
            ItemData item = registry.FindItem(entry.itemName);
            if (item == null)
            {
                Debug.LogWarning($"[PlayerInventory] Unknown item '{entry.itemName}' in save — skipped.");
                continue;
            }
            _items[item] = entry.quantity;
        }
        OnChanged?.Invoke();
    }

    #endregion
}
