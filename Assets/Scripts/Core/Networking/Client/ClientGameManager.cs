using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class ClientGameManager : IDisposable
{
    private JoinAllocation allocation;
    private NetworkClient networkClient;

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        networkClient = new NetworkClient(NetworkManager.Singleton);

        AuthState authState = await AuthenticationWrapper.DoAuth();

        await VivoxService.Instance.InitializeAsync();
        Debug.Log("Vivox logging in...");
        await VivoxService.Instance.LoginAsync();
        Debug.Log("Vivox logged in!");
        if (authState == AuthState.Authenticated)
        {
            return true;
        }

        return false;
    }
    public async Task<Lobby> GetLobbyByJoinCode(string joinCode)
    {
        return await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
    }

    public async Task StartClientAsync(string relayJoinCode)
    {
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);


        TransmitUserData();


        NetworkManager.Singleton.StartClient();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
    public void Disconnect()
    {
        networkClient.Disconnect();
    }

    public void TransmitUserData()
    {
        UserData userData = new UserData
        {
            UserAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
    }


}
