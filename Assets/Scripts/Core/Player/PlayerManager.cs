using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private GameObject players;


    public event Action OnAllPlayersSpawnInTheGame;

    public event Action OnResetALlPlayerPosition;

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
                SetParentForPlayers();
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

    public GameObject GetPlayersGameObject()
    {
        return players;
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
                    player.GetComponent<NetworkObject>().TrySetParent(players, true);
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
        OnResetALlPlayerPosition?.Invoke();
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
                OnResetALlPlayerPosition?.Invoke();
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
        voteManager.OnStateChanged -= OnVoteStateChangedServerRpc;
    }

    private void SpawnAllPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
            playerNetworkObject.SpawnAsPlayerObject(clientId, true);
            playerNetworkObject.TrySetParent(players, true);

        }
    }
    public void ClearAllPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject && client.PlayerObject.TryGetComponent<Player>(out var player))
                {
                    player.GetComponent<NetworkObject>().Despawn(true);
                }
            }
        }
    }
}
