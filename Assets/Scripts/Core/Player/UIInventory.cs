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
        int itemCount = inv.GetItemList().Count;

        for (int i = 0; i < ItemSlots.Length; i++)
        {
            if (i < itemCount)
            {
                ItemSlots[i].GetComponent<Image>().sprite = inv.GetItemList()[i].GetSprite(); //Set the item in the slot
            }
            else
            {
                ItemSlots[i].GetComponent<Image>().sprite = null; //Clear the slot if no item is present
            }
        }
        
    }
}
