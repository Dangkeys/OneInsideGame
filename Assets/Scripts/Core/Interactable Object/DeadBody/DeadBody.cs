using Unity.Netcode;
using UnityEngine;

public class DeadBody : NetworkBehaviour
{
    public NetworkVariable<ulong> DeadBodyOwnerID = new NetworkVariable<ulong>();
    [field: SerializeField] public GameObject DeadBodyPlayerVisual { get; private set; }

    public void OnInteract(InteractionData interactionData)
    {
        if (gameObject.TryGetComponent<NetworkObject>(out NetworkObject deadBodyNetworkObject)
            && interactionData.Interactor.TryGetComponent<NetworkObject>(out NetworkObject interactorNetworkObject))
        {
            ulong interactorID = interactorNetworkObject.OwnerClientId;

            Debug.Log($"Player {interactorID} found body of {DeadBodyOwnerID.Value}");

            deadBodyNetworkObject.Despawn();
            OneInsideLevelManager.Instance.VoteManager.RaiseVoteStartServerRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(PlayerManager.GetPlayerByClientId(DeadBodyOwnerID.Value).OwnerClientId);
        Debug.Log(PlayerManager.GetPlayerByClientId(DeadBodyOwnerID.Value).CharacterName.Value.ToString());
        CharacterManager.ChangeChracter(DeadBodyPlayerVisual, PlayerManager.GetPlayerByClientId(DeadBodyOwnerID.Value).CharacterName.Value.ToString());
    }
}
