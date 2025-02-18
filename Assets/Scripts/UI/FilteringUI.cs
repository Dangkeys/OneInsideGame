using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilteringUI : MonoBehaviour
{
    public LobbyQueryDto LobbyQueryDto { get; private set; } = new LobbyQueryDto(OneInside.Constants.Lobby.DEFAULT_PAGE_SIZE);

    [SerializeField] private Toggle isfilterByGameMode;
    [SerializeField] private Toggle isfilterByPlayerAmount;
    [SerializeField] private Toggle isfilterByImposterAmount;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_Dropdown gameModeDropdown;
    
    [SerializeField] private Button decreasePlayerAmount;
    [SerializeField] private Button increasePlayerAmount;
    [SerializeField] private TextMeshProUGUI playerAmountText;
    
    [SerializeField] private Button decreaseImposterAmount;
    [SerializeField] private Button increaseImposterAmount;
    [SerializeField] private TextMeshProUGUI imposterAmountText;
    
    [SerializeField] private Button applyFilterLobbyButton;
    
    private int playerCount = OneInside.Constants.Player.MIN_PLAYERS;
    private int imposterCount = OneInside.Constants.Player.MIN_IMPOSTERS;

    private void Start()
    {
        UpdateCountTexts();
        SetupButtons();
        SetupToggles();
        UpdateInteractableStates();
    }

    private void SetupToggles()
    {
        isfilterByGameMode.onValueChanged.AddListener(OnGameModeFilterChanged);
        isfilterByPlayerAmount.onValueChanged.AddListener(OnPlayerAmountFilterChanged);
        isfilterByImposterAmount.onValueChanged.AddListener(OnImposterAmountFilterChanged);
    }

    private void OnGameModeFilterChanged(bool isOn)
    {
        gameModeDropdown.interactable = isOn;
    }

    private void OnPlayerAmountFilterChanged(bool isOn)
    {
        decreasePlayerAmount.interactable = isOn;
        increasePlayerAmount.interactable = isOn;
        playerAmountText.color = isOn ? Color.white : Color.gray;
    }

    private void OnImposterAmountFilterChanged(bool isOn)
    {
        decreaseImposterAmount.interactable = isOn;
        increaseImposterAmount.interactable = isOn;
        imposterAmountText.color = isOn ? Color.white : Color.gray;
    }

    private void UpdateInteractableStates()
    {
        OnGameModeFilterChanged(isfilterByGameMode.isOn);
        OnPlayerAmountFilterChanged(isfilterByPlayerAmount.isOn);
        OnImposterAmountFilterChanged(isfilterByImposterAmount.isOn);
    }

    private void SetupButtons()
    {
        decreasePlayerAmount.onClick.AddListener(() =>
        {
            if (playerCount > OneInside.Constants.Player.MIN_PLAYERS)
            {
                playerCount--;
                if (imposterCount >= playerCount)
                {
                    imposterCount = playerCount - 1;
                }
                UpdateCountTexts();
            }
        });
        
        increasePlayerAmount.onClick.AddListener(() =>
        {
            if (playerCount < OneInside.Constants.Player.MAX_PLAYERS)
            {
                playerCount++;
                UpdateCountTexts();
            }
        });
        
        decreaseImposterAmount.onClick.AddListener(() =>
        {
            if (imposterCount > OneInside.Constants.Player.MIN_IMPOSTERS)
            {
                imposterCount--;
                UpdateCountTexts();
            }
        });
        
        increaseImposterAmount.onClick.AddListener(() =>
        {
            if (imposterCount < OneInside.Constants.Player.MAX_IMPOSTERS && imposterCount < playerCount - 1)
            {
                imposterCount++;
                UpdateCountTexts();
            }
        });
        
        applyFilterLobbyButton.onClick.AddListener(ApplyFilter);
    }

    private void UpdateCountTexts()
    {
        playerAmountText.text = playerCount.ToString();
        imposterAmountText.text = imposterCount.ToString();
    }

    private void ApplyFilter()
    {
        LobbyQueryDto.LobbyName = lobbyNameInputField.text;
        LobbyQueryDto.GameMode = isfilterByGameMode ? (GameMode)gameModeDropdown.value : null;
        LobbyQueryDto.MaxPlayers = isfilterByPlayerAmount ? playerCount : null;
        LobbyQueryDto.ImposterAmount = isfilterByImposterAmount ? imposterCount : null;
    }
}