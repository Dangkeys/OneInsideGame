using Unity.Netcode;
using UnityEngine;

public class batHit : MonoBehaviour, IInteractable
{
    public NetworkObject bat;
    public void Interact()
    {
        batHitsServerRPC();
    }

    [ServerRpc]
    private void batHitsServerRPC(){
        if (bat.transform.parent != null && bat.transform.parent.TryGetComponent<Player>(out Player player) && bat.OwnerClientId == player.OwnerClientId) //trying to compare the interactor and the bat owner is the same guy (Fixing)
        {
            Debug.Log("The bat owner is " + bat.OwnerClientId);
        }
    }


}
