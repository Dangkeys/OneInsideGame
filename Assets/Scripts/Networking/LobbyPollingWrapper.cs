using System;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public static class LobbyPollingWrapper
{
    private static bool isPolling = false;
    private static string currentLobbyId;
    private static readonly float lobbyPollTime = 1.1f;

    public static event Action<Lobby> OnLobbyUpdated;

    public static async Task StartPollingLobby(string lobbyId)
    {
        if (isPolling && currentLobbyId == lobbyId) return;

        currentLobbyId = lobbyId;
        isPolling = true;

        while (isPolling)
        {
            try
            {
                var lobby = await PollLobby();
                if (lobby != null)
                {
                    OnLobbyUpdated?.Invoke(lobby);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to poll lobby: {e.Message}");
                StopPolling();
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(lobbyPollTime));
        }
    }

    private static async Task<Lobby> PollLobby()
    {
        if (string.IsNullOrEmpty(currentLobbyId)) return null;

        try
        {
            return await LobbyService.Instance.GetLobbyAsync(currentLobbyId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error polling lobby {currentLobbyId}: {e.Message}");
            return null;
        }
    }

    public static void StopPolling()
    {
        isPolling = false;
        currentLobbyId = null;
    }
}