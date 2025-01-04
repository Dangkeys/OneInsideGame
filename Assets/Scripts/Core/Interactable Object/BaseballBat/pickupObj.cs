using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PickupObj : NetworkBehaviour, IInteractable
{
    private bool haveObj;
    public void Interact(InteractionData interactionData)
    {
        if (interactionData.Interactor.TryGetComponent<Player>(out Player player))
        {
            if (player.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                ulong InteractorID = networkObject.NetworkObjectId;
                PickupObjServerRPC(InteractorID); //pass the id
            }

        }
    }

    [ServerRpc(RequireOwnership = false)]

    private void PickupObjServerRPC(ulong interactorID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(interactorID, out NetworkObject interactorObj)) //SpawnedObjects is dict with key as id and value as the obj
        {
            GameObject interactor = interactorObj.gameObject; //change id into its gameobject
            if (!haveObj) //pickup
            {
                if (interactor.TryGetComponent<Player>(out Player player))
                {
                    NetworkObject.ChangeOwnership(player.OwnerClientId);
                    NetworkObject.transform.parent = player.transform;
                    haveObj = true;
                }
            }
            else //drop
            {
                if (interactor.TryGetComponent<Player>(out Player player) && NetworkObject.OwnerClientId == player.OwnerClientId)
                {
                    NetworkObject.RemoveOwnership();
                    NetworkObject.transform.parent = null;
                    haveObj = false;
                }
            }
        }

    }

}
