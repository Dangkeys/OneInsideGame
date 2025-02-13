using UnityEngine;

public class LobbyResponseDto
{
    public LobbyResponseDto(string lobbyName, GameMode gameMode, int maxPlayers, int imposterAmount, string lobbyJoinCode)
    {
        LobbyName = lobbyName;
        GameMode = gameMode;
        MaxPlayers = maxPlayers;
        ImposterAmount = imposterAmount;
        LobbyJoinCode = lobbyJoinCode;
    }

    public string LobbyName { get; set; }

    public GameMode GameMode { get; set; }

    public int MaxPlayers { get; set; }

    public int ImposterAmount { get; set; }

    public string LobbyJoinCode { get; set; }
}
