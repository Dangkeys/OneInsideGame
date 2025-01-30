using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private bool shouldSpawnPlayers = false;


    public event Action OnSetAllPlayersToSpawnPos;

    public event Action<bool> OnEnableAllPlayersMovement;



    public override void OnNetworkSpawn()
    {
        if (!OneInsideLevelManager.Instance)
            return;
        if (IsServer)
        {
            OneInsideLevelManager.Instance.OnGameStart += HandleGameStart;
        }
        OneInsideLevelManager.Instance.VoteManager.OnStateChanged += OnVoteStateChangedServerRpc;
    }

    private void HandleGameStart()
    {

        if (shouldSpawnPlayers)
        {
            SpawnAllPlayers();
        }
        SetAllPlayerToSpawnPosClientRpc();
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
    private void SetAllPlayerToSpawnPosClientRpc()
    {
        OnSetAllPlayersToSpawnPos?.Invoke();
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
                OnSetAllPlayersToSpawnPos?.Invoke();
                break;
            case VoteManager.State.VoteOver:
                OnEnableAllPlayersMovement?.Invoke(true);
                break;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
        }

        if (OneInsideLevelManager.Instance)
        {
            OneInsideLevelManager.Instance.VoteManager.OnStateChanged -= OnVoteStateChangedServerRpc;
        }
    }
}