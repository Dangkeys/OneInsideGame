using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
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

    private void Start()
    {
        StartGameButton.onClick.AddListener(StartGame);
        LeaveRoomButton.onClick.AddListener(LeaveRoom);
        ReadyButton.onClick.AddListener(Ready);
        MainMenuUI.Instance.OnLobbyValueChanged += OnLobbyValueChanged;
        gameObject.SetActive(false);
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
        }
    }

    private async void OnEnable()
    {
        if (MainMenuUI.Instance.Lobby == null)
            return;

        LobbyPollingWrapper.OnLobbyUpdated += OnLobbyUpdated;
        UpdateUI();
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
        JoinCodeText.text = $"JoinCode: {MainMenuUI.Instance.Lobby.Data["JoinCode"].Value}";

        foreach (Transform child in PlayerItemParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var player in MainMenuUI.Instance.Lobby.Players)
        {
            PlayerItem playerItem = Instantiate(PlayerItemPrefab, PlayerItemParent);
            playerItem.Initialize(player.Id);
        }
    }

    private void StartGame()
    {
        //TODO: Load game scene
    }

    private void LeaveRoom()
    {
        //TODO: Leave room implementation
    }

    private void Ready()
    {
        //TODO: Ready implementation
    }
}