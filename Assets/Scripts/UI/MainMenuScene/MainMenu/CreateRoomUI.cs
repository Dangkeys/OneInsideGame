using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;

public class CreateRoomUI : MonoBehaviour
{
    [field: SerializeField] public TMP_InputField RoomNameInputField { get; private set; }

    [field: SerializeField] public Button IncreasePlayerAmountButton { get; private set; }
    [field: SerializeField] public Button DecreasePlayerAmountButton { get; private set; }
    [field: SerializeField] public TMP_Text PlayerAmountText { get; private set; }

    [field: SerializeField] public Toggle IsPrivateToggle { get; private set; }
    [field: SerializeField] public Button CreateRoomButton { get; private set; }
    [field: SerializeField] public MainMenuUI MainMenuUI { get; private set; }
    public event Action<Lobby> OnRoomCreated;
    private int playerAmount = LobbyCustomization.MIN_PLAYERS;

    private void Start()
    {
        IncreasePlayerAmountButton.onClick.AddListener(IncreasePlayerAmount);
        DecreasePlayerAmountButton.onClick.AddListener(DecreasePlayerAmount);
        CreateRoomButton.onClick.AddListener(CreateRoom);
        RoomNameInputField.onValueChanged.AddListener(OnRoomNameChanged);
        MainMenuUI.Instance.OnLobbyValueChanged += OnLobbyValueChanged;

        UpdatePlayerAmountUI();
        UpdateButtonInteractability();
    }
    private void OnDestroy() {
        MainMenuUI.Instance.OnLobbyValueChanged -= OnLobbyValueChanged;
    }
    private void OnLobbyValueChanged(Lobby lobby)
    {
        if (lobby == null)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnRoomNameChanged(string newValue)
    {
        CreateRoomButton.interactable = !string.IsNullOrWhiteSpace(newValue);
    }

    private async void CreateRoom()
    {
        if (string.IsNullOrWhiteSpace(RoomNameInputField.text))
        {
            Debug.LogWarning("Room name cannot be empty");
            return;
        }

        LobbyConfig config = new LobbyConfig
        {
            RoomName = RoomNameInputField.text.Trim(),
            MaxPlayerAmount = playerAmount,
            IsPrivate = IsPrivateToggle.isOn
        };

        try
        {
            CreateRoomButton.interactable = false;
            Lobby lobby = await HostSingleton.Instance.GameManager.StartHostAsync(config);
            gameObject.SetActive(false);
            OnRoomCreated?.Invoke(lobby);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            CreateRoomButton.interactable = true;
        }
    }

    private void DecreasePlayerAmount()
    {
        if (playerAmount > LobbyCustomization.MIN_PLAYERS)
        {
            playerAmount--;
            UpdatePlayerAmountUI();
            UpdateButtonInteractability();
        }
    }

    private void IncreasePlayerAmount()
    {
        if (playerAmount < LobbyCustomization.MAX_PLAYERS)
        {
            playerAmount++;
            UpdatePlayerAmountUI();
            UpdateButtonInteractability();
        }
    }

    private void UpdatePlayerAmountUI()
    {
        PlayerAmountText.text = playerAmount.ToString();
    }

    private void UpdateButtonInteractability()
    {
        DecreasePlayerAmountButton.interactable = playerAmount > LobbyCustomization.MIN_PLAYERS;
        IncreasePlayerAmountButton.interactable = playerAmount < LobbyCustomization.MAX_PLAYERS;

        // Ensure create button is only enabled if room name is valid
        CreateRoomButton.interactable = !string.IsNullOrWhiteSpace(RoomNameInputField.text);
    }
}