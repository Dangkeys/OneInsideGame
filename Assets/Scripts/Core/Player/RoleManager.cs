using System;
using System.Collections.Generic;
using Mono.CSharp;
using Unity.Netcode;
using UnityEngine;

public class RoleManager : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        OneInsideLevelManager.Instance.PlayerManager.OnAllPlayersInTheGame += HandleAllPlayersInTheGame;
    }

    private void HandleAllPlayersInTheGame(){
        if(IsServer){
            AssignRandomRoles(1);
        }
    }

    public override void OnNetworkDespawn()
    {
        OneInsideLevelManager.Instance.PlayerManager.OnAllPlayersInTheGame -= HandleAllPlayersInTheGame;
    }


    private void AssignRandomRoles(int imposterCount)
    {
        var players = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        imposterCount = Mathf.Clamp(imposterCount, 0, players.Count);
        var shuffledPlayers = ShuffleUtility.GetShuffledList(players);

        for (int i = 0; i < shuffledPlayers.Count; i++)
        {
            var role = i < imposterCount ? PlayerRole.Imposter : PlayerRole.Crewmate;
            var clientId = shuffledPlayers[i];

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject.TryGetComponent<Player>(out var player))
                {
                    player.Role.Value = role;
                }
            }

        }
    }

}