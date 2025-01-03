using System;
using QFSW.QC.Actions;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : Singleton<MainMenuUI>
{
    [field: SerializeField] public CreateRoomUI CreateRoomUI { get; private set; }
    [field: SerializeField] public WaitingRoomUI WaitingRoomUI { get; private set; }
    [field: SerializeField] public FindMatchUI FindMatchUI { get; private set; }
    public event Action<Lobby> OnLobbyValueChanged;

    public LobbyConfig LobbyConfig { get; private set; } = new LobbyConfig();
    public Lobby Lobby { get; private set; } = null;
    private const string GAME_SCENE = "TajdangScene";

    private void Start()
    {
        CreateRoomUI.OnRoomCreated += OnRoomCreated;
        FindMatchUI.OnLobbyJoined += OnLobbyJoined;
    }

    private void OnDestroy()
    {
        CreateRoomUI.OnRoomCreated -= OnRoomCreated;
        FindMatchUI.OnLobbyJoined -= OnLobbyJoined;
    }
    private void OnRoomCreated(Lobby lobby)
    {
        SetLobby(lobby);
    }

    private void OnLobbyJoined(Lobby lobby)
    {
        SetLobby(lobby);
    }

    public void SetLobby(Lobby lobby)
    {
        Lobby = lobby;
        OnLobbyValueChanged?.Invoke(Lobby);
    }
}