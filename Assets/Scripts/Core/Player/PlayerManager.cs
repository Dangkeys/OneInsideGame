using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{
    public event Action OnSetAllPlayersToSpawnPos;
    public event Action<bool> OnEnableAllPlayersMovement;
    [field: SerializeField] public Transform PlayerPrefab { get; private set; }
    [field: SerializeField] private bool shouldSpawnPlayers = false;


    public override void OnNetworkSpawn()
    {


        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadComplete;

        if (!OneInsideLevelManager.Instance)
            return;
        OneInsideLevelManager.Instance.VoteManager.OnStateChanged += OnVoteStateChangedServerRpc;
    }

    private void OnSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != "TajdangScene" || !IsServer)
            return;
        if (!shouldSpawnPlayers)
            return;
        foreach (ulong id in clientsCompleted)
        {
            GameObject player = Instantiate(PlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnVoteStateChangedServerRpc(VoteManager.State state)
    {
        OnVoteStateChangedClientRpc(state);
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