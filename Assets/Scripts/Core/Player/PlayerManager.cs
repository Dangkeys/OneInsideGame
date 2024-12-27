using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetAllPlayerToSpawnPointServerRPC()
    {
        Debug.Log(NetworkManager.Singleton.LocalClientId);
        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (var player in players)
        {
            if (player.TryGetComponent<NetworkObject>(out var networkObject))
            {
                ResetPlayerToSpawnPointClientRPC(networkObject.NetworkObjectId);
            }
        }
    }

    [ClientRpc]
    public void ResetPlayerToSpawnPointClientRPC(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
            playerNetworkId, out NetworkObject networkObject))
        {
            if (networkObject.TryGetComponent<Player>(out var player))
            {
                player.ResetToSpawnPoint();
            }
        }
    }
}