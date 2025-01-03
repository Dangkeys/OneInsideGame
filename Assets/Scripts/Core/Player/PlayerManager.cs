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
    public NetworkVariable<List<ulong>> PlayerClientIds = new NetworkVariable<List<ulong>>(new List<ulong>());
    [field: SerializeField] private bool shouldSpawnPlayers = false;

    private Dictionary<ulong, NetworkObject> spawnedPlayers = new Dictionary<ulong, NetworkObject>();

    public override void OnNetworkSpawn()
    {
        if (!OneInsideLevelManager.Instance)
            return;

        NetworkManager.Singleton.OnConnectionEvent += NetworkOnConnectionEvent;
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;

        OneInsideLevelManager.Instance.VoteManager.OnStateChanged += OnVoteStateChangedServerRpc;
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.UnloadComplete)
        {
            if (IsServer)
            {
                foreach (var player in spawnedPlayers.Values)
                {
                    if (player != null && player.IsSpawned)
                    {
                        player.Despawn(true);
                    }
                }
                spawnedPlayers.Clear();
            }
        }
        else if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            if (IsServer)
            {
                SpawnAllPlayers();
            }
        }
    }

    private void SpawnAllPlayers()
    {
        if (!IsServer)

            Debug.Log("SpawnAllPlayers");
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer || !shouldSpawnPlayers)
            return;

        if (spawnedPlayers.TryGetValue(clientId, out NetworkObject existingPlayer))
        {
            if (existingPlayer != null && existingPlayer.IsSpawned)
            {
                existingPlayer.Despawn(true);
            }
            spawnedPlayers.Remove(clientId);
        }


        if (!PlayerClientIds.Value.Contains(clientId))
        {
            PlayerClientIds.Value.Add(clientId);
        }

        Vector3 spawnPosition = SpawnPoint.GetClientSpawnPos(clientId);
        Transform playerTransform = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = playerTransform.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        spawnedPlayers[clientId] = networkObject;
    }

    private void NetworkOnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        switch (data.EventType)
        {
            case ConnectionEvent.ClientConnected:
                if (IsServer)
                {
                    SpawnPlayer(data.ClientId);
                }
                break;
            case ConnectionEvent.ClientDisconnected:
                if (IsServer)
                {
                    if (spawnedPlayers.TryGetValue(data.ClientId, out NetworkObject player))
                    {
                        if (player != null && player.IsSpawned)
                        {
                            player.Despawn(true);
                        }
                        spawnedPlayers.Remove(data.ClientId);
                    }
                    if (PlayerClientIds.Value.Contains(data.ClientId))
                    {
                        PlayerClientIds.Value.Remove(data.ClientId);
                    }
                }
                break;
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
            NetworkManager.Singleton.OnConnectionEvent -= NetworkOnConnectionEvent;
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
        }

        if (OneInsideLevelManager.Instance)
        {
            OneInsideLevelManager.Instance.VoteManager.OnStateChanged -= OnVoteStateChangedServerRpc;
        }
        if (IsServer)
        {
            foreach (var player in spawnedPlayers.Values)
            {
                if (player != null && player.IsSpawned)
                {
                    player.Despawn(true);
                }
            }
            spawnedPlayers.Clear();
        }
    }
}