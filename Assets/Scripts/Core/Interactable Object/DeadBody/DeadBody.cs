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
            OneInsideLevelSystem.Instance.VoteManager.RaiseVoteStartServerRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(PlayerSystem.GetPlayerByClientId(DeadBodyOwnerID.Value).OwnerClientId);
        Debug.Log(PlayerSystem.GetPlayerByClientId(DeadBodyOwnerID.Value).CurrentCharacterID.Value.ToString());
        CharacterManager.Instance.ChangeChracter(DeadBodyPlayerVisual, PlayerSystem.GetPlayerByClientId(DeadBodyOwnerID.Value).CurrentCharacterID.Value.ToString());
    }
}
