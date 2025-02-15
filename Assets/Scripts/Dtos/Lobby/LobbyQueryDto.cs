using Unity.Services.Lobbies.Models;
using UnityEngine;

#nullable enable

public class LobbyQueryDto : PaginationQueryDto
{
    public LobbyQueryDto(int pageSize, string? continuationToken = null) : base(pageSize, continuationToken)
    {
    }

    public LobbyQueryDto(string? lobbyName, GameMode? gameMode, int? maxPlayers, int? imposterAmount,int pageSize, string? continuationToken = null) : base(pageSize, continuationToken)
    {
        LobbyName = lobbyName;
        GameMode = gameMode;
        MaxPlayers = maxPlayers;
        ImposterAmount = imposterAmount;
    }
    public string? LobbyName { get; set; }

    public GameMode? GameMode { get; set; }

    public int? MaxPlayers { get; set; }

    public int? ImposterAmount { get; set; }
}
