using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private TextMeshProUGUI impostorAmountText;
    [SerializeField] private Button joinButton;

    private Lobby lobby;

    public void Initialize(Lobby lobby)
    {
        this.lobby = lobby;

        lobbyNameText.text = lobby.Name;
        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers} players";

        if (lobby.Data != null &&
            lobby.Data.TryGetValue(OneInside.Constants.Lobby.KEY_GAME_MODE, out var gameModeData))
        {
            gameModeText.text = gameModeData.Value;
        }
        if (lobby.Data != null &&
    lobby.Data.TryGetValue(OneInside.Constants.Lobby.KEY_IMPOSTER_AMOUNT, out var keyImposterAmount))
        {
            impostorAmountText.text = $"Impostors: {keyImposterAmount.Value}";
        }

        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnJoinClicked()
    {
        _ = JoinMatchAsync();
    }
    
    private async Task JoinMatchAsync()
    {
        await OneInsideGameManager.Instance.JoinMatchByLobbyIdAsync(lobby.Id);
    }

    private void OnDestroy()
    {
        joinButton.onClick.RemoveListener(OnJoinClicked);
    }
}