using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class NetcodeManager : NetworkBehaviour
{
    private OneInsideGameManager gameManager;
    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent += NetworkManager_OnConnectionEvent;
        }
        gameManager = OneInsideGameManager.Instance;

    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManger_ConnectionApprovalCallback;
        }
    }

    private void NetworkManger_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if(NetworkManager.Singleton.ConnectedClientsList.Count >= OneInsideGameManager.Instance.LobbyManager.CurrentLobby.MaxPlayers)
        {
            response.Approved = false;
        }
        else
        {
            response.Approved = true;
        }
    }

    private void NetworkManager_OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        switch (data.EventType)
        {
            case ConnectionEvent.ClientConnected:
                if (data.ClientId == localClientId)
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        Debug.Log("You started hosting the server!");
                    }
                    else
                    {
                        OneInsideGameManager.Instance.ShowProgressChanged(1f, "Connected to server");
                        Debug.Log($"Connected to server! Our ID: {data.ClientId}");
                        if (data.PeerClientIds.IsCreated)
                        {
                            Debug.Log("Other players in game:");
                            foreach (var peerId in data.PeerClientIds)
                            {
                                Debug.Log($"- Player ID: {peerId}");
                            }
                        }
                    }
                }
                else if (NetworkManager.Singleton.IsServer)
                {
                    Debug.Log($"Client connected! ID: {data.ClientId}");
                }
                break;

            case ConnectionEvent.ClientDisconnected:
                if (data.ClientId == localClientId)
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        Debug.Log("You stopped hosting the server!");
                        Loader.Load(GameScene.MainMenuScene);
                        OneInsideGameManager.Instance.ShowProgressChanged(1f, "Match Left");
                        gameManager.ShowMessage("You stopped hosting the server!");
                    }
                    else
                    {
                        Debug.Log("Disconnected from the server!");
                        Loader.Load(GameScene.MainMenuScene);
                        OneInsideGameManager.Instance.ShowProgressChanged(1f, "Match Left");
                        gameManager.ShowMessage("Disconnected from the server!");
                    }
                }
                else if (NetworkManager.Singleton.IsServer)
                {
                    Debug.Log($"Client disconnected! ID: {data.ClientId}");
                }
                break;

            case ConnectionEvent.PeerConnected:
                if (!NetworkManager.Singleton.IsServer)
                {
                    Debug.Log($"New peer connected! ID: {data.ClientId}");
                }
                break;

            case ConnectionEvent.PeerDisconnected:
                if (!NetworkManager.Singleton.IsServer)
                {
                    Debug.Log($"Peer disconnected! ID: {data.ClientId}");
                }
                break;
        }
    }
    public void MigrateHost()
    {
        //TODO Implement host migration(I dont think we can do this with the current version of unity)
    }


    public void LeaveMatch()
    {
        NetworkManager.Singleton.Shutdown();
    }
    public void KickPlayer(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
            NetworkManager.Singleton.DisconnectClient(clientId);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback = NetworkManger_ConnectionApprovalCallback;
        }
    }
    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent -= NetworkManager_OnConnectionEvent;
        }
    }
    public static void InitializeHostRelayTransport(CreateLobbyAllocationResponseDto allocationResponse)
    {
        if (allocationResponse?.Allocation == null)
            throw new ArgumentNullException(nameof(allocationResponse));

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocationResponse.Allocation, "dtls");
        transport.SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();
    }

    public static void InitializeClientRelayTransport(JoinLobbyAllocationResponseDto allocationResponse)
    {
        if (allocationResponse?.JoinAllocation == null)
            throw new ArgumentNullException(nameof(allocationResponse));

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocationResponse.JoinAllocation, "dtls");
        transport.SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();
    }

}