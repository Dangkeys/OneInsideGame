using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReadyManager : NetworkBehaviour
{
    public static ReadyManager Instance { get; private set; }
    public event Action<bool> OnAllPlayersReadyChanged;
    public event Action<Dictionary<ulong, bool>> OnReadyStateChanged;

    private NetworkVariable<Dictionary<ulong, bool>> readyRegistry = new NetworkVariable<Dictionary<ulong, bool>>(
        new Dictionary<ulong, bool>()
    );

    private bool previousAllPlayersReady = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeReadyDictionary();
            NetworkManager.Singleton.OnConnectionEvent += NetworkOnConnectionEvent;
        }

        readyRegistry.OnValueChanged += ReadyDictionaryChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnConnectionEvent -= NetworkOnConnectionEvent;
        }
        readyRegistry.OnValueChanged -= ReadyDictionaryChanged;
    }

    private void NetworkOnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        if (!IsServer)
            return;

        Dictionary<ulong, bool> newDictionary = new Dictionary<ulong, bool>(readyRegistry.Value);

        switch (data.EventType)
        {
            case ConnectionEvent.ClientConnected:
                if (!newDictionary.ContainsKey(data.ClientId))
                {
                    newDictionary[data.ClientId] = false;
                }
                break;

            case ConnectionEvent.ClientDisconnected:
                newDictionary.Remove(data.ClientId);
                break;
        }

        readyRegistry.Value = newDictionary;
    }

    private void ReadyDictionaryChanged(Dictionary<ulong, bool> previousValue, Dictionary<ulong, bool> newValue)
    {
        bool allPlayersReady = newValue.Count > 0 && newValue.All(kvp => kvp.Value);

        // Notify subscribers that ready states have changed
        OnReadyStateChanged?.Invoke(newValue);

        // Only notify if the all-ready state has changed
        if (allPlayersReady != previousAllPlayersReady)
        {
            previousAllPlayersReady = allPlayersReady;
            OnAllPlayersReadyChanged?.Invoke(allPlayersReady);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(bool isReady, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        Dictionary<ulong, bool> newDictionary = new Dictionary<ulong, bool>(readyRegistry.Value)
        {
            [clientId] = isReady
        };
        readyRegistry.Value = newDictionary;
    }
    [ClientRpc]
    public void StartGameClientRpc()
    {
        LobbyPollingWrapper.StopPolling();
    }
    public bool IsPlayerReady(ulong clientId)
    {
        return readyRegistry.Value.TryGetValue(clientId, out bool isReady) && isReady;
    }


    public bool AreAllPlayersReady()
    {
        return readyRegistry.Value.Count > 0 && readyRegistry.Value.All(kvp => kvp.Value);
    }


    private void InitializeReadyDictionary()
    {
        Dictionary<ulong, bool> newDictionary = NetworkManager.Singleton.ConnectedClientsIds
            .ToDictionary(clientId => clientId, _ => false);
        readyRegistry.Value = newDictionary;
    }
    public Dictionary<ulong, bool> GetCurrentReadyStates()
    {
        return readyRegistry.Value;
    }
}