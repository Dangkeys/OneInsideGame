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

public class ClientGameManager : IDisposable
{
    private JoinAllocation allocation;
    private NetworkClient networkClient;
    private const string MainMenuSceneName = "MainMenuScene";

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        networkClient = new NetworkClient(NetworkManager.Singleton);

        AuthState authState = await AuthenticationWrapper.DoAuth();

        if (authState == AuthState.Authenticated)
        {
            return true;
        }

        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }
    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        UserData userData = new UserData
        {
            UserName = "Tajdang",
            UserAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;


        NetworkManager.Singleton.StartClient();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }

}
