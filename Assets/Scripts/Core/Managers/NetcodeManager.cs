using System;
using System.Text;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class NetcodeManager : NetworkBehaviour
{
    public NetworkPlayerData NetworkPlayerData { get; private set; }
    private OneInsideGameManager gameManager;

    void Awake()
    {
        NetworkPlayerData = GetComponent<NetworkPlayerData>();
    }
    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent += NetworkManager_OnConnectionEvent;
            NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManger_ConnectionApprovalCallback;
        }
        gameManager = OneInsideGameManager.Instance;

    }

    private void NetworkManger_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var currentLobby = OneInsideGameManager.Instance.LobbyManager.CurrentLobby;
        if (currentLobby == null)
        {
            var levelManager = OneInsideLevelManager.Instance;
            if (levelManager == null)
            {
                response.Approved = false;
                return;
            }
            if (levelManager != null && levelManager.State.Value != GameState.WaitingToStart)
            {
                response.Approved = false;
                return;
            }
        }

        if (currentLobby != null && (NetworkManager.Singleton.ConnectedClientsList.Count >= currentLobby.MaxPlayers || currentLobby.IsLocked))
        {
            response.Approved = false;
            return;
        }

        string payload = Encoding.UTF8.GetString(request.Payload);
        UserDataDto userData = JsonUtility.FromJson<UserDataDto>(payload);

        Debug.Log(NetworkPlayerData.ClientIdToAuth);
        NetworkPlayerData.ClientIdToAuth.Value[request.ClientNetworkId] = userData.AuthId;
        NetworkPlayerData.AuthIdToUserData.Value[userData.AuthId] = userData;
        Debug.Log(NetworkPlayerData.AuthIdToUserData.Value[userData.AuthId]);
        response.Approved = true;

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
                        if (OneInsideGameManager.Instance.LobbyManager.CurrentLobby != null)
                            Loader.Load(GameScene.MainMenuScene);
                        OneInsideGameManager.Instance.ShowProgressChanged(1f, "Match Left");
                        gameManager.ShowMessage("You stopped hosting the server!");
                    }
                    else
                    {
                        if (OneInsideGameManager.Instance.LobbyManager.CurrentLobby != null)
                            Loader.Load(GameScene.MainMenuScene);
                        OneInsideGameManager.Instance.ShowProgressChanged(1f, "Match Left");
                        if (manager.DisconnectReason != "")
                        {
                            gameManager.ShowMessage(manager.DisconnectReason);
                        }
                        else
                        {
                            gameManager.ShowMessage("Disconnected from server");
                        }
                    }
                }
                else if (NetworkManager.Singleton.IsServer)
                {

                    if (NetworkPlayerData.ClientIdToAuth.Value.ContainsKey(data.ClientId))
                    {
                        FixedString32Bytes authId = NetworkPlayerData.ClientIdToAuth.Value[data.ClientId];
                        if (NetworkPlayerData.AuthIdToUserData.Value.ContainsKey(authId))
                        {
                            NetworkPlayerData.AuthIdToUserData.Value.Remove(authId);
                        }
                        // Remove the client ID mapping
                        NetworkPlayerData.ClientIdToAuth.Value.Remove(data.ClientId);
                    }
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

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent -= NetworkManager_OnConnectionEvent;
            NetworkManager.Singleton.ConnectionApprovalCallback -= NetworkManger_ConnectionApprovalCallback;
        }
    }
    public static void InitializeHostRelayTransport(CreateLobbyAllocationResponseDto allocationResponse)
    {
        if (allocationResponse?.Allocation == null)
            throw new ArgumentNullException(nameof(allocationResponse));

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocationResponse.Allocation, "dtls");
        transport.SetRelayServerData(relayServerData);
        TransmitUserData();


        NetworkManager.Singleton.StartHost();
    }

    public static void InitializeClientRelayTransport(JoinLobbyAllocationResponseDto allocationResponse)
    {
        if (allocationResponse?.JoinAllocation == null)
            throw new ArgumentNullException(nameof(allocationResponse));

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocationResponse.JoinAllocation, "dtls");
        transport.SetRelayServerData(relayServerData);
        TransmitUserData();

        NetworkManager.Singleton.StartClient();
    }

    public static void TransmitUserData()
    {
        var userDataDto = new UserDataDto
        {
            AuthId = AuthenticationService.Instance.PlayerId,
            Name = AuthenticationService.Instance.PlayerName
        };

        string payload = JsonUtility.ToJson(userDataDto);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

    }

}