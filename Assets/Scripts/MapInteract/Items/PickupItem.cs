using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PickupItem : NetworkBehaviour,IInteractable
{
    public void Interact(InteractionData interactionData)
    {
        if (interactionData.Interactor.TryGetComponent<Player>(out Player player))
        {
            player.Inventory.AddItem(gameObject.GetComponent<ItemWorld>().GetItem()); //Add the item to the inventory
            if(!player.Inventory.IsFull) Destroy(gameObject); //Destroy the item in the world
        }
    }
}
