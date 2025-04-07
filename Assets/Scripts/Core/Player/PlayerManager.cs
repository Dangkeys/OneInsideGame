using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField] private Transform playerPrefab;


    public event Action OnAllPlayersSpawnInTheGame;

    public event Action OnResetAllPlayerPosition;

    public event Action<bool> OnEnableAllPlayersMovement;
    private OneInsideLevelManager oneInsideLevelManager;

    private VoteManager voteManager;

    void Awake()
    {
        oneInsideLevelManager = OneInsideLevelManager.Instance;
        voteManager = oneInsideLevelManager.VoteManager;
    }


    public override void OnNetworkSpawn()
    {
        if (oneInsideLevelManager == null)
            return;
        if (IsServer)
        {
            oneInsideLevelManager.State.OnValueChanged += HandleGameStateChanged;
        }
        voteManager.OnStateChanged += OnVoteStateChangedServerRpc;
    }

    private void HandleGameStateChanged(GameState previousValue, GameState newValue)
    {
        switch (newValue)
        {
            case GameState.WaitingToStart:
                break;
            case GameState.GamePlaying:
                SpawnAllPlayers();
                ResetAllPlayerPositionClientRpc();
                if (!IsHost)
                {
                    OnAllPlayersSpawnInTheGame?.Invoke();
                }
                else
                {
                    OnAllPlayersInTheGameClientRpc();
                }

                break;
            case GameState.GameOver:
                break;
        }
    }

    private void SetParentForPlayers()
    {
        if (!IsServer)
            return;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject && client.PlayerObject.TryGetComponent<Player>(out var player))
                {
                    player.GetComponent<NetworkObject>().TrySetParent(OneInsideLevelManager.Players, true);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnVoteStateChangedServerRpc(VoteState state)
    {
        OnVoteStateChangedClientRpc(state);
    }

    [ClientRpc]
    private void ResetAllPlayerPositionClientRpc()
    {
        OnResetAllPlayerPosition?.Invoke();
    }

    [ClientRpc]
    private void OnAllPlayersInTheGameClientRpc()
    {
        OnAllPlayersSpawnInTheGame?.Invoke();
    }


    [ClientRpc]
    private void OnVoteStateChangedClientRpc(VoteState state)
    {
        switch (state)
        {
            case VoteState.WaitingToVote:
                break;
            case VoteState.Voting:
                OnEnableAllPlayersMovement?.Invoke(false);
                OnResetAllPlayerPosition?.Invoke();
                break;
            case VoteState.VoteOver:
                OnEnableAllPlayersMovement?.Invoke(true);
                break;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (oneInsideLevelManager == null)
            return;
        if (IsServer)
        {
            oneInsideLevelManager.State.OnValueChanged -= HandleGameStateChanged;
        }
        OneInsideLevelManager.Instance.VoteManager.OnStateChanged -= OnVoteStateChangedServerRpc;
    }
    private void SpawnAllPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
            playerNetworkObject.SpawnAsPlayerObject(clientId);

        }
    }
    public void ClearAllPlayers()
    {
        var players = GetAllPlayer(player => true);
        var playerObjects = NetworkManager.Singleton.ConnectedClientsList.Select(client => client.PlayerObject).ToList();
        foreach (var player in players)
        {
            player.UnSubscribeEvent();
        }
        foreach (var playerObject in playerObjects)
        {
            if (!IsServer)
                return;
            playerObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
            playerObject.Despawn();
        }
    }
    public static NetworkObject GetLocalPlayer()
    {
        return GetPlayerByClientId(NetworkManager.Singleton.LocalClientId);
    }


    public static Player GetPlayerScriptByClientId(ulong clientId)
    {
        return GetPlayerByClientId(clientId).GetComponent<Player>();
    }

    public static Player GetLocalPlayerScript()
    {
        return GetLocalPlayer().GetComponent<Player>();
    }

    public static NetworkObject GetPlayerByClientId(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            return client.PlayerObject;
        }
        return null;
    }
    public static List<Player> GetAllPlayer(Func<Player, bool> filter)
    {
        List<Player> players = new List<Player>();
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject && client.PlayerObject.TryGetComponent<Player>(out var player))
                {
                    if (filter == null || filter(player))
                    {
                        players.Add(player);
                    }
                }
            }
        }
        return players;
    }

    public static List<Player> GetSpectatorPlayers(bool includeSelf = false)
    {
        return GetAllPlayer(player => !player.GetComponent<Player>().IsAlive.Value && (includeSelf || !player.IsOwner));
    }

    public static Player GetPlayerByClientId(ulong clientId)
    {
        return GetAllPlayer((player) => player.OwnerClientId == clientId)[0];
    }

    public static GameObject GetPlayerVisual(Player player)
    {
        return player.PlayerVisual;
    }
}
