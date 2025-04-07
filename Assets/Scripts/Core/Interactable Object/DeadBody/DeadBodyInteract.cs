using Unity.Netcode;
using UnityEngine;

public class DeadBodyInteract : NetworkBehaviour, IInteractable
{
    [SerializeField] private GameObject deadBody;

    public void Interact(InteractionData interactionData)
    {
        if (deadBody.TryGetComponent<NetworkObject>(out NetworkObject deadBodyNetworkObject)
            && interactionData.InteractorGameObject.TryGetComponent<NetworkObject>(out NetworkObject interactorNetworkObject))
        {
            ulong deadBodyOwnerID = deadBodyNetworkObject.OwnerClientId;
            ulong interactorID = interactorNetworkObject.OwnerClientId;

            Debug.Log($"Player {interactorID} found body of {deadBodyOwnerID}");

            OneInsideLevelManager.Instance.VoteManager.RaiseVoteStartServerRpc();
        }
    }

    public bool CanInteract(InteractionData interactionData)
    {
        if (interactionData.InteractorGameObject.TryGetComponent<Player>(out Player player))
        {
            return player.IsAlive.Value;
        }
        return false;
    }
}
