using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoleManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            OneInsideLevelManager.Instance.OnGameStart += HandleGameStart;
        }
    }

    private void HandleGameStart()
    {
        AssignRandomRoles(1);
    }

    private void AssignRandomRoles(int imposterCount)
    {
        var players = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        imposterCount = Mathf.Clamp(imposterCount, 0, players.Count);
        var shuffledPlayers = ShuffleUtility.GetShuffledList(players);

        for (int i = 0; i < shuffledPlayers.Count; i++)
        {
            var role = i < imposterCount ? Player.Role.Imposter : Player.Role.Crewmate;
            var clientId = shuffledPlayers[i];

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject.TryGetComponent<Player>(out var player))
                {
                    player.SetRoleServerRpc(role);
                }
            }

        }
    }
    public override void OnDestroy()
    {
        if (IsServer)
        {
            OneInsideLevelManager.Instance.OnGameStart -= HandleGameStart;
        }
    }

}