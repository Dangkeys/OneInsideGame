using Unity.Netcode;
using UnityEngine;

public class BatHit : NetworkBehaviour//, IInteractable
{
    // public NetworkObject Bat;
    // public void Interact(InteractionData interactionData)
    // {
    //     BatHitsServerRPC();
    // }

    // [ServerRpc]
    // private void BatHitsServerRPC(){
    //     if (Bat.transform.parent != null && Bat.transform.parent.TryGetComponent<Player>(out Player player) && Bat.OwnerClientId == player.OwnerClientId) //trying to compare the interactor and the Bat owner is the same guy (Fixing)
    //     {
    //         Debug.Log("The Bat owner is " + Bat.OwnerClientId);
    //     }
    // }


}
