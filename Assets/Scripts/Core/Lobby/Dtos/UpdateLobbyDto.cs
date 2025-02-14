#nullable enable
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class UpdateLobbyDto
{
    public UpdateLobbyDto(string? lobbyName, GameMode? gameMode, int? maxPlayers, int? imposterAmount, bool? isPrivate, string hostId)
    {
        LobbyName = lobbyName;
        GameMode = gameMode;
        MaxPlayers = maxPlayers;
        ImposterAmount = imposterAmount;
        IsPrivate = isPrivate;
        HostId = hostId;
    }
    public string? HostId { get; set; }
    public string? LobbyName { get; set; }

    public GameMode? GameMode { get; set; }

    public int? MaxPlayers { get; set; }

    public int? ImposterAmount { get; set; }
    public bool? IsPrivate { get; set; }
}
