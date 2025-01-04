using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class WaitingRoomUI : MonoBehaviour
{
    [field: SerializeField] public TextMeshProUGUI RoomNameText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI PlayerAmountText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI JoinCodeText { get; private set; }
    [field: SerializeField] public Button StartGameButton { get; private set; }
    [field: SerializeField] public Button LeaveRoomButton { get; private set; }
    [field: SerializeField] public Button ReadyButton { get; private set; }
    [field: SerializeField] public Transform PlayerItemParent { get; private set; }
    [field: SerializeField] public PlayerItem PlayerItemPrefab { get; private set; }
    private const string GAME_SCENE = "TajdangScene";
    private bool isReady = false;

    private void Start()
    {
        StartGameButton.onClick.AddListener(StartGame);
        LeaveRoomButton.onClick.AddListener(LeaveRoom);
        ReadyButton.onClick.AddListener(Ready);
        MainMenuUI.Instance.OnLobbyValueChanged += OnLobbyValueChanged;
        ReadyManager.Instance.OnAllPlayersReadyChanged += OnAllPlayersReadyChanged;
        gameObject.SetActive(false);
    }

    private void OnAllPlayersReadyChanged(bool isAllPlayerReady)
    {
        if (!isAllPlayerReady)
        {
            return;
        }
        if (!NetworkManager.Singleton.IsListening)
        {
            return;
        }
        if (MainMenuUI.Instance.Lobby.Players.Count < LobbyCustomization.MIN_PLAYERS ||
             MainMenuUI.Instance.Lobby.Players.Count > LobbyCustomization.MAX_PLAYERS)
        {
            return;
        }
        if (MainMenuUI.Instance.Lobby.Players.Count == MainMenuUI.Instance.Lobby.MaxPlayers)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                StartGame();
            }
        }
    }

    private void OnLobbyValueChanged(Lobby lobby)
    {
        if (lobby == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);

            if (MainMenuUI.Instance.Lobby.Players.Count < LobbyCustomization.MIN_PLAYERS ||
                MainMenuUI.Instance.Lobby.Players.Count > LobbyCustomization.MAX_PLAYERS)
            {
                StartGameButton.interactable = false;
            }
            else
            {
                StartGameButton.interactable = true;
            }
        }
    }

    private async void OnEnable()
    {
        if (MainMenuUI.Instance.Lobby == null)
            return;

        LobbyPollingWrapper.OnLobbyUpdated += OnLobbyUpdated;
        UpdateUI();
        if (!NetworkManager.Singleton.IsHost)
        {
            StartGameButton.gameObject.SetActive(false);
        }
        await LobbyPollingWrapper.StartPollingLobby(MainMenuUI.Instance.Lobby.Id);
    }

    private void OnDisable()
    {
        LobbyPollingWrapper.OnLobbyUpdated -= OnLobbyUpdated;
        LobbyPollingWrapper.StopPolling();
    }

    private void OnDestroy()
    {
        MainMenuUI.Instance.OnLobbyValueChanged -= OnLobbyValueChanged;
    }

    private void OnLobbyUpdated(Lobby lobby)
    {
        MainMenuUI.Instance.SetLobby(lobby);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (MainMenuUI.Instance.Lobby == null)
            return;
        RoomNameText.text = MainMenuUI.Instance.Lobby.Name;
        PlayerAmountText.text = $"{MainMenuUI.Instance.Lobby.Players.Count}/{MainMenuUI.Instance.Lobby.MaxPlayers} Players";
        JoinCodeText.text = $"JoinCode: {MainMenuUI.Instance.Lobby.LobbyCode}";

        foreach (Transform child in PlayerItemParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            PlayerItem playerItem = Instantiate(PlayerItemPrefab, PlayerItemParent);
            playerItem.Initialize(player.ClientId.ToString());
        }
    }

    private void StartGame()
    {

        if (NetworkManager.Singleton.IsHost)
        {
            ReadyManager.Instance.StartGameClientRpc();
            HostSingleton.Instance.GameManager.DeleteLobbyAsync();
            NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE, LoadSceneMode.Single);
        }

    }

    private void LeaveRoom()
    {
        MainMenuUI.Instance.SetLobby(null);
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.DeleteLobbyAsync();
            HostSingleton.Instance.GameManager.Shutdown();
        }
        else if(NetworkManager.Singleton.IsClient)
        {
            ClientSingleton.Instance.GameManager.Disconnect();
        }
    }

    private void Ready()
    {
        isReady = !isReady;
        ReadyButton.GetComponentInChildren<TextMeshProUGUI>().text = isReady ? "Not Ready" : "Ready";
        ReadyManager.Instance.SetPlayerReadyServerRpc(isReady);
    }
}