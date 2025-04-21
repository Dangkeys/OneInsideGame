using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public bool IsFull = false;
    private List<Item> itemList = new List<Item>();
    public static event Action<Inventory> OnRefreshInventory;

    public Inventory()
    {
        
    }

    public void AddItem(Item item)
    {
        if(itemList.Count > 5)
        {
            Debug.Log("Inventory is full!");
            IsFull = true;
            return;
        }
        itemList.Add(item);
        OnRefreshInventory?.Invoke(this);
    }

    public List<Item> GetItemList()
    {
        return itemList;
    }
}
