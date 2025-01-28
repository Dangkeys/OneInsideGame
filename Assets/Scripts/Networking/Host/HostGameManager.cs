using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using Unity.Services.Authentication;

public class HostGameManager : IDisposable
{
    private Allocation allocation;
    private string relayJoinCode;
    private string lobbyId;
    public NetworkServer NetworkServer { get; private set; }
    public async Task<Lobby> StartHostAsync(LobbyConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.RoomName))
        {
            throw new ArgumentException("Room name cannot be null or empty");
        }

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(config.MaxPlayerAmount);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create allocation: {e}");
            throw;
        }

        try
        {
            relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get join code: {e}");
            throw;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        Lobby lobby;
        try
        {
            var lobbyOptions = LobbyCustomization.GenerateCreateLobbyOptions(config, relayJoinCode);

            lobby = await LobbyService.Instance.CreateLobbyAsync(
                config.RoomName,
                config.MaxPlayerAmount,
                lobbyOptions);

            lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HearbeatLobby(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e}");
            throw;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        UserData userData = new UserData
        {
            UserName = "Tajdang",
            UserAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartHost();
        NetworkServer.OnClientLeft += HandleClientLeft;
        return lobby;
    }

    private async void HandleClientLeft(string authId)
    {
        if (string.IsNullOrEmpty(lobbyId))
        {
            return;
        }
        try
        {

            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    private IEnumerator HearbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
    public void Dispose()
    {
        Shutdown();
    }

    public void Shutdown()
    {
        NetworkServer?.Dispose();
    }
    public async void DeleteLobbyAsync()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HearbeatLobby));

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }

            lobbyId = string.Empty;
        }
    }
}