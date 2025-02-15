#nullable enable
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class UpdateLobbyDto
{
    public UpdateLobbyDto(bool? isLocked)
    {
        IsLocked = isLocked;
    }
    public UpdateLobbyDto(string? lobbyName, GameMode? gameMode, int? maxPlayers, int? imposterAmount, bool? isPrivate, bool? isLocked = false)
    {
        LobbyName = lobbyName;
        GameMode = gameMode;
        MaxPlayers = maxPlayers;
        ImposterAmount = imposterAmount;
        IsPrivate = isPrivate;
        IsLocked = isLocked;
    }
    public string? LobbyName { get; set; }

    public GameMode? GameMode { get; set; }

    public int? MaxPlayers { get; set; }

    public int? ImposterAmount { get; set; }
    public bool? IsPrivate { get; set; }
    public bool? IsLocked { get; set; }
}
