using Unity.Services.Lobbies.Models;
using UnityEngine;

#nullable enable

public class LobbyQueryDto
{
    public LobbyQueryDto(string? lobbyName, GameMode? gameMode, int? maxPlayers, int? imposterAmount, string? lobbyJoinCode, string? isPrivate)
    {
        LobbyName = lobbyName;
        GameMode = gameMode;
        MaxPlayers = maxPlayers;
        ImposterAmount = imposterAmount;
        LobbyJoinCode = lobbyJoinCode;
        IsPrivate = isPrivate;
    }
    public string? LobbyName { get; set; }

    public GameMode? GameMode { get; set; }

    public int? MaxPlayers { get; set; }

    public int? ImposterAmount { get; set; }

    public string? LobbyJoinCode { get; set; }

    public string? IsPrivate { get; set; }
}
