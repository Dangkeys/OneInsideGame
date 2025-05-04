using System;
using Unity.Netcode;
using UnityEngine;

public class Crafting : NetworkBehaviour
{
    public static event Action OnCraftItemA;

    public void CraftItemA()
    {
        OnCraftItemA?.Invoke();
    }
}
