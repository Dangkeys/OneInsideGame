using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<Item> itemList = new List<Item>();

    public Inventory()
    {
        AddItem(new Item { IType = Item.ItemType.IngredientA });
        AddItem(new Item { IType = Item.ItemType.IngredientB });
        AddItem(new Item { IType = Item.ItemType.IngredientC });
    }

    public void AddItem(Item item)
    {
        if(itemList.Count > 5)
        {
            Debug.Log("Inventory is full!");
            return;
        }
        itemList.Add(item);
    }

    public List<Item> GetItemList()
    {
        return itemList;
    }
}
