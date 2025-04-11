using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ViewPlayersUI : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private ReadySystem readyManager;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private PlayerListItemUI playerPrefab;
    private OneInsideGameManager oneInsideGameManager;
    private NetworkPlayerData networkPlayerData;
    private bool isReady = false;

    private Dictionary<ulong, PlayerListItemUI> playerListItems = new Dictionary<ulong, PlayerListItemUI>();

    void Awake()
    {
        oneInsideGameManager = OneInsideGameManager.Instance;
        networkPlayerData = ConnectionManager.Instance.NetworkPlayerData;
    }

    void Start()
    {
        SetupServerButtons();
        SetupEvents();
        UpdateAllPlayers();
    }

    private void SetupServerButtons()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            startGameButton.gameObject.SetActive(false);

        }
        else
        {

            startGameButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowConfirmation("Are you sure you want to start the game?", async () =>
                    {
                        UIManager.Instance.ShowProgressChanged(.5f, "Starting Game...");
                        if (NetworkManager.Singleton.ConnectedClients.Count >=
                        (LobbyManager.Instance.CurrentLobby.Data.TryGetValue(
                            OneInside.Constants.Lobby.KEY_IMPOSTER_AMOUNT, out var imposterAmount) ?
                        int.Parse(imposterAmount.Value) : OneInside.Constants.Player.MIN_IMPOSTERS))
                        {

                            await oneInsideGameManager.StartGame();
                        }
                        else
                        {
                            UIManager.Instance.ShowProgressChanged(1f, "Cannot start game, not enough players");
                            UIManager.Instance.ShowMessage("Cannot start game, not enough players");
                        }
                    }, null);
            });

        }

        readyButton.onClick.AddListener(() =>
        {
            isReady = !isReady;
            readyManager.SetPlayerReadyServerRpc(isReady);
        });
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent += NetworkManager_OnConnectionEvent;
        }
    }

    private void NetworkManager_OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        switch (data.EventType)
        {
            case ConnectionEvent.ClientConnected:
                UpdateAllPlayers();
                CanServerStartGame();
                break;
            case ConnectionEvent.ClientDisconnected:
                UpdateAllPlayers();
                CanServerStartGame();
                break;
            case ConnectionEvent.PeerConnected:
                UpdateAllPlayers();
                break;
            case ConnectionEvent.PeerDisconnected:
                UpdateAllPlayers();
                break;
        }
    }

    private void SetupEvents()
    {
        readyManager.OnReadyStateChanged += ReadyManager_OnReadyStateChanged;
    }
    private void ReadyManager_OnReadyStateChanged(Dictionary<ulong, bool> readyStates)
    {
        UpdatePlayerReadyStates(readyStates);
    }

    private void UpdateAllPlayers()
    {
        var connectedClients = NetworkManager.Singleton.ConnectedClientsIds;
        var readyStates = readyManager.GetCurrentReadyStates();

        List<ulong> playersToRemove = playerListItems.Keys
            .Where(clientId => !connectedClients.Contains(clientId))
            .ToList();

        foreach (ulong clientId in playersToRemove)
        {
            if (playerListItems.TryGetValue(clientId, out PlayerListItemUI item))
            {
                Destroy(item.gameObject);
                playerListItems.Remove(clientId);
            }
        }

        foreach (ulong clientId in connectedClients)
        {
            if (!playerListItems.TryGetValue(clientId, out PlayerListItemUI playerItem))
            {
                playerItem = Instantiate(playerPrefab, playerContainer);
                playerListItems[clientId] = playerItem;
            }

            bool isReady = readyStates.TryGetValue(clientId, out bool readyState) && readyState;
            string playerName = GetPlayerName(clientId);
            playerItem.Initilize(playerName, isReady);
        }
    }

    private void UpdatePlayerReadyStates(Dictionary<ulong, bool> readyStates)
    {
        foreach (var playerState in readyStates)
        {
            ulong clientId = playerState.Key;
            bool isReady = playerState.Value;

            if (playerListItems.TryGetValue(clientId, out PlayerListItemUI playerItem))
            {
                string playerName = GetPlayerName(clientId);
                playerItem.Initilize(playerName, isReady);
            }
        }
    }

    private string GetPlayerName(ulong clientId)
    {
        if (networkPlayerData != null &&
            networkPlayerData.ClientIdToAuth.Value.TryGetValue(clientId, out FixedString32Bytes authId) &&
            networkPlayerData.AuthIdToUserData.Value.TryGetValue(authId, out UserDataDto userData))
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                return $"{userData.Name} (You)";
            }
            return userData.Name.ToString();
        }

        return clientId == NetworkManager.Singleton.LocalClientId ? "You" : $"Player {clientId}";
    }

    private void CanServerStartGame()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        startGameButton.interactable = NetworkManager.Singleton.ConnectedClientsList.Count >= OneInside.Constants.Player.MIN_PLAYERS;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent -= NetworkManager_OnConnectionEvent;
        }

        if (readyManager != null)
        {
            readyManager.OnReadyStateChanged -= ReadyManager_OnReadyStateChanged;
        }

        foreach (var item in playerListItems.Values)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        playerListItems.Clear();
    }
}