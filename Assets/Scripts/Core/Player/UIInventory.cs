using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIInventory : NetworkBehaviour
{
    public GameObject[] ItemSlots;
    public override void OnNetworkSpawn()
    {
        Inventory.OnRefreshInventory += RefreshInventory;
    }

    private void RefreshInventory(Inventory inv)
    {
        Inventory inventory = inv;
        int i=0; //Indicate each slot in the inventory
        foreach (Item item in inventory.GetItemList())
        {
            if(item != null)
            {
                ItemSlots[i].GetComponent<Image>().sprite = item.GetSprite(); //Set the item in the slot
                i++;
            }
        }
    }
}
