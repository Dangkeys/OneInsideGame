using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CreateLobbyDto
{
    public CreateLobbyDto(string lobbyName, GameMode gameMode, int maxPlayers, int imposterAmount, string relayJoinCode, bool isPrivate)
    {
        LobbyName = lobbyName;
        GameMode = gameMode;
        MaxPlayers = maxPlayers > OneInside.Constants.Player.MAX_PLAYERS ? OneInside.Constants.Player.MAX_PLAYERS : maxPlayers; 
        ImposterAmount = imposterAmount;
        RelayJoinCode = relayJoinCode;
        IsPrivate = isPrivate;
    }

    public string LobbyName { get; set; }

    public GameMode GameMode { get; set; }

    public int MaxPlayers { get; set; }

    public int ImposterAmount { get; set; }

    public string RelayJoinCode { get; set; }  

    public bool IsPrivate { get; set; }
}
