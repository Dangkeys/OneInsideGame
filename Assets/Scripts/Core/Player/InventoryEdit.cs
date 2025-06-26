using System;
using Unity.Netcode;
using UnityEngine;

public class InventoryEdit : NetworkBehaviour
{
    public static event Action OnCraftItemA;
    public static event Action OnDropItem;

    public void CraftItemA()
    {
        OnCraftItemA?.Invoke();
    }

    public void DropItem()
    {
        OnDropItem?.Invoke();
    }
}
