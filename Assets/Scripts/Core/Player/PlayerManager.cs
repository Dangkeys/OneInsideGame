using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField] private Transform playerPrefab;


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
                if (!OneInsideLevelManager.Instance.IsPlayerInitializationRequired.Value)
                {
                    SpawnAllPlayers();
                }
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

    private void SpawnAllPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void OnVoteStateChangedServerRpc(VoteManager.State state)
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
    private void OnVoteStateChangedClientRpc(VoteManager.State state)
    {
        switch (state)
        {
            case VoteManager.State.WaitingToVote:
                break;
            case VoteManager.State.Voting:
                OnEnableAllPlayersMovement?.Invoke(false);
                OnResetALlPlayerPosition?.Invoke();
                break;
            case VoteManager.State.VoteOver:
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