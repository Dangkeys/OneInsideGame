using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private GameObject players;


    public event Action OnAllPlayersInTheGame;

    public event Action OnResetALlPlayerPosition;

    public event Action<bool> OnEnableAllPlayersMovement;


    public override void OnNetworkSpawn()
    {
        if (!OneInsideLevelManager.Instance)
            return;
        if (IsServer)
        {
            OneInsideLevelManager.Instance.State.OnValueChanged += HandleGameStateChanged;
        }
        OneInsideLevelManager.Instance.VoteManager.OnStateChanged += OnVoteStateChangedServerRpc;
    }

    private void HandleGameStateChanged(GameState previousValue, GameState newValue)
    {
        switch (newValue)
        {
            case GameState.WaitingToStart:
                break;
            case GameState.GamePlaying:
                SetParentForPlayers();
                ResetAllPlayerPositionClientRpc();
                if (!IsHost)
                {
                    OnAllPlayersInTheGame?.Invoke();
                }
                else
                {
                    OnAllPlayersInTheGameClientRpc();
                }

                break;
            case GameState.GameOver:
                ResetAllPlayerPositionClientRpc();
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
        OnAllPlayersInTheGame?.Invoke();
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
        if (!OneInsideLevelManager.Instance)
            return;
        if (IsServer)
        {
            OneInsideLevelManager.Instance.State.OnValueChanged -= HandleGameStateChanged;
        }
        OneInsideLevelManager.Instance.VoteManager.OnStateChanged -= OnVoteStateChangedServerRpc;
    }
}
