using System;
using System.Collections.Generic;
using Mono.CSharp;
using Unity.Netcode;
using UnityEngine;

public class RoleSystem : NetworkBehaviour
{

    PlayerSystem playerManager;
    public event Action OnRolesAssignmentComplete;
    void Awake()
    {
        playerManager = OneInsideLevelSystem.Instance.PlayerSystem;
    }
    public override void OnNetworkSpawn()
    {
        playerManager.OnAllPlayersSpawnInTheGame += HandleAllPlayersInTheGame;
    }

    private void HandleAllPlayersInTheGame()
    {
        if (IsServer)
        {
            var currentLobby = LobbyManager.Instance.CurrentLobby;
            if (currentLobby != null)
            {
                AssignRandomRoles(currentLobby.Data.TryGetValue(OneInside.Constants.Lobby.KEY_IMPOSTER_AMOUNT, out var imposters)
                    ? int.Parse(imposters.Value)
                    : OneInside.Constants.Player.MIN_IMPOSTERS);
            }
            else
            {
                //TODO add configuration
                AssignRandomRoles(OneInsideLevelSystem.Instance.ImposterAmount);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        playerManager.OnAllPlayersSpawnInTheGame -= HandleAllPlayersInTheGame;
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
        OnRolesAssignmentComplete?.Invoke();
    }

}