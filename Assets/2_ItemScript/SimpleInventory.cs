using UnityEngine;
using System;
using System.Collections.Generic;

public class SimpleInventory : MonoBehaviour
{
    [SerializeField] private int maxSlots = 3;
    private List<ItemIconData> items = new List<ItemIconData>();

    public event Action OnInventoryChanged;

    public bool AddItem(ItemIconData item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("インベントリ満タン！");
            return false;
        }

        items.Add(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        items.RemoveAt(index);
        OnInventoryChanged?.Invoke();
    }

    public List<ItemIconData> GetItems()
    {
        return items;
    }

    public int GetMaxSlots() => maxSlots;

    public void ExpandSlots(int amount)
    {
        maxSlots += amount;
        OnInventoryChanged?.Invoke();
    }
}