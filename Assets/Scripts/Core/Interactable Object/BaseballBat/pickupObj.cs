using Unity.Netcode;
using UnityEngine;

public class PickupObj : NetworkBehaviour
{
    // public NetworkObject bat;
    // private void OnTriggerEnter(Collider other){
    //     PickupObjServerRPC(other);
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void PickupObjServerRPC(Collider other){ //Still have error "Only server can reparent and change ownership but it can still change it?
    //     if(other.TryGetComponent<Player>(out Player player) && bat.transform.parent == null){
    //         ulong clientId = player.OwnerClientId;
    //         bat.transform.parent = player.transform;
    //         bat.ChangeOwnership(clientId);
    //         Debug.Log("Bat owner is " + bat.OwnerClientId);
    //     }
    // }
}
