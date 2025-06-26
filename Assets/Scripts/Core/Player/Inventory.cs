using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
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
    public void CraftItemA(){

        bool hasIngredientA = false;
        bool hasIngredientB = false;

        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].IType == Item.ItemType.IngredientA)
            {
                hasIngredientA = true;
                break;
            }
        }
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].IType == Item.ItemType.IngredientB)
            {
                hasIngredientB = true;
                break;
            }
        }
        if(hasIngredientA == false || hasIngredientB == false)
        {
            return;
        }

        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].IType == Item.ItemType.IngredientA)
            {
                itemList.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].IType == Item.ItemType.IngredientB)
            {
                itemList.RemoveAt(i);
                break;
            }
        }
    
        itemList.Add(new Item { IType = Item.ItemType.ItemA });
        OnRefreshInventory?.Invoke(this);
    }

    public Item.ItemType DropItem()
    {
        if (itemList.Count > 0)
        {
            Item.ItemType itemToDrop = itemList[itemList.Count - 1].IType;
            itemList.RemoveAt(itemList.Count - 1);
            OnRefreshInventory?.Invoke(this);

            return itemToDrop;
        }
        return Item.ItemType.None;
    }

    public List<Item> GetItemList()
    {
        return itemList;
    }
}
