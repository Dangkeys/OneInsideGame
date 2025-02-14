using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CreateLobbyDto
{
    public CreateLobbyDto(string lobbyName, GameMode gameMode, int maxPlayers, int imposterAmount, bool isPrivate)
    {
        LobbyName = lobbyName;
        GameMode = gameMode;
        MaxPlayers = Mathf.Clamp(maxPlayers, OneInside.Constants.Player.MIN_PLAYERS, OneInside.Constants.Player.MAX_PLAYERS); 
        ImposterAmount = imposterAmount;
        IsPrivate = isPrivate;
    }

    public string LobbyName { get; set; }

    public GameMode GameMode { get; set; }

    public int MaxPlayers { get; set; }

    public int ImposterAmount { get; set; }


    public bool IsPrivate { get; set; }
}
