using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UpdateLobbyUI : MonoBehaviour
{
    [SerializeField] private Toggle isPrivateToggle;
    [SerializeField] private TMPro.TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_Dropdown gameModeDropdown;

    [SerializeField] private Button decreasePlayerAmount;
    [SerializeField] private Button increasePlayerAmount;
    [SerializeField] private TextMeshProUGUI playerAmountText;

    [SerializeField] private Button decreaseImposterAmount;
    [SerializeField] private Button increaseImposterAmount;
    [SerializeField] private TextMeshProUGUI imposterAmountText;

    [SerializeField] private Button updateLobbyButton;

    private int playerCount;
    private int imposterCount;
    private int currentConnectedPlayers;
    private bool isInitialized;

    private void Start()
    {
        InitializeValues();
        SetupButtons();
        SetupInputValidation();
        UpdateButtonStates();
    }

    private void InitializeValues()
    {
        var lobby = OneInsideGameManager.Instance.LobbyManager.CurrentLobby;
        if (lobby == null)
        {
            Debug.LogError("Current lobby is null!");
            return;
        }

        // Initialize connected players count
        currentConnectedPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
        
        // Initialize player count from current lobby settings
        playerCount = lobby.MaxPlayers;
        
        // Initialize imposter count from lobby data
        imposterCount = lobby.Data.TryGetValue(OneInside.Constants.Lobby.KEY_IMPOSTER_AMOUNT, out var imposters) 
            ? int.Parse(imposters.Value) 
            : OneInside.Constants.Player.MIN_IMPOSTERS;

        // Initialize UI elements from current lobby settings
        lobbyNameInputField.text = lobby.Name;
        isPrivateToggle.isOn = lobby.IsPrivate;
        
        if (lobby.Data.TryGetValue(OneInside.Constants.Lobby.KEY_GAME_MODE, out var gameMode))
        {
            gameModeDropdown.value = (int)System.Enum.Parse(typeof(GameMode), gameMode.Value);
        }

        UpdateCountTexts();
        isInitialized = true;
    }

    private void SetupButtons()
    {
        decreasePlayerAmount.onClick.AddListener(() =>
        {
            if (playerCount > GetMinimumAllowedPlayers())
            {
                playerCount--;
                if (imposterCount >= playerCount)
                {
                    imposterCount = playerCount - 1;
                }
                UpdateCountTexts();
                UpdateButtonStates();
            }
        });

        increasePlayerAmount.onClick.AddListener(() =>
        {
            if (playerCount < OneInside.Constants.Player.MAX_PLAYERS)
            {
                playerCount++;
                UpdateCountTexts();
                UpdateButtonStates();
            }
        });

        decreaseImposterAmount.onClick.AddListener(() =>
        {
            if (imposterCount > OneInside.Constants.Player.MIN_IMPOSTERS)
            {
                imposterCount--;
                UpdateCountTexts();
                UpdateButtonStates();
            }
        });

        increaseImposterAmount.onClick.AddListener(() =>
        {
            if (imposterCount < OneInside.Constants.Player.MAX_IMPOSTERS && imposterCount < playerCount - 1)
            {
                imposterCount++;
                UpdateCountTexts();
                UpdateButtonStates();
            }
        });

        updateLobbyButton.onClick.AddListener(UpdateLobby);
    }

    private void SetupInputValidation()
    {
        lobbyNameInputField.onValueChanged.AddListener(_ => ValidateLobbyName());
    }

    private int GetMinimumAllowedPlayers()
    {
        return Mathf.Max(OneInside.Constants.Player.MIN_PLAYERS, currentConnectedPlayers);
    }

    private void UpdateButtonStates()
    {
        if (!isInitialized) return;

        // Update player amount buttons
        decreasePlayerAmount.interactable = playerCount > GetMinimumAllowedPlayers();
        increasePlayerAmount.interactable = playerCount < OneInside.Constants.Player.MAX_PLAYERS;

        // Update imposter amount buttons
        decreaseImposterAmount.interactable = imposterCount > OneInside.Constants.Player.MIN_IMPOSTERS;
        increaseImposterAmount.interactable = imposterCount < OneInside.Constants.Player.MAX_IMPOSTERS 
            && imposterCount < playerCount - 1;

        // Update the update lobby button
        updateLobbyButton.interactable = IsValidConfiguration();
    }

    private void UpdateCountTexts()
    {
        playerAmountText.text = playerCount.ToString();
        imposterAmountText.text = imposterCount.ToString();
    }

    private bool IsValidConfiguration()
    {
        if (string.IsNullOrWhiteSpace(lobbyNameInputField.text)) return false;
        if (playerCount < GetMinimumAllowedPlayers()) return false;
        if (playerCount > OneInside.Constants.Player.MAX_PLAYERS) return false;
        if (imposterCount >= playerCount) return false;
        if (imposterCount < OneInside.Constants.Player.MIN_IMPOSTERS) return false;
        if (imposterCount > OneInside.Constants.Player.MAX_IMPOSTERS) return false;
        
        return true;
    }

    private void ValidateLobbyName()
    {
        var isValid = !string.IsNullOrWhiteSpace(lobbyNameInputField.text);
        lobbyNameInputField.GetComponent<Image>().color = isValid ? Color.white : Color.red;
        UpdateButtonStates();
    }

    private async void UpdateLobby()
    {
        if (!IsValidConfiguration())
        {
            OneInsideGameManager.Instance.ShowMessage("Invalid lobby configuration!");
            return;
        }

        try
        {
            // Disable UI during update
            SetUIInteractable(false);

            var updateLobbyDto = new UpdateLobbyDto(
                lobbyNameInputField.text.Trim(),
                (GameMode)gameModeDropdown.value,
                playerCount,
                imposterCount,
                isPrivateToggle.isOn
            );

            var updatedLobby = await OneInsideGameManager.Instance.LobbyManager.UpdateCurrentLobbyAsync(updateLobbyDto);
            
            if (updatedLobby == null)
            {
                OneInsideGameManager.Instance.ShowMessage("Failed to update lobby!");
                return;
            }

            OneInsideGameManager.Instance.ShowMessage("Lobby updated successfully!");
        }
        catch (System.Exception e)
        {
            OneInsideGameManager.Instance.ShowMessage($"Error updating lobby: {e.Message}");
            Debug.LogError($"Error updating lobby: {e}");
        }
        finally
        {
            // Re-enable UI after update
            SetUIInteractable(true);
        }
    }

    private void SetUIInteractable(bool interactable)
    {
        isPrivateToggle.interactable = interactable;
        lobbyNameInputField.interactable = interactable;
        gameModeDropdown.interactable = interactable;
        decreasePlayerAmount.interactable = interactable;
        increasePlayerAmount.interactable = interactable;
        decreaseImposterAmount.interactable = interactable;
        increaseImposterAmount.interactable = interactable;
        updateLobbyButton.interactable = interactable;
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (lobbyNameInputField != null)
            lobbyNameInputField.onValueChanged.RemoveAllListeners();
        
        if (decreasePlayerAmount != null)
            decreasePlayerAmount.onClick.RemoveAllListeners();
        
        if (increasePlayerAmount != null)
            increasePlayerAmount.onClick.RemoveAllListeners();
        
        if (decreaseImposterAmount != null)
            decreaseImposterAmount.onClick.RemoveAllListeners();
        
        if (increaseImposterAmount != null)
            increaseImposterAmount.onClick.RemoveAllListeners();
        
        if (updateLobbyButton != null)
            updateLobbyButton.onClick.RemoveAllListeners();
    }
}