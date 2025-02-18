using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartHostUI : MonoBehaviour
{
    [SerializeField] private Toggle isPrivateToggle;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_Dropdown gameModeDropdown;

    [SerializeField] private Button decreasePlayerAmount;
    [SerializeField] private Button increasePlayerAmount;
    [SerializeField] private TextMeshProUGUI playerAmountText;

    [SerializeField] private Button decreaseImposterAmount;
    [SerializeField] private Button increaseImposterAmount;
    [SerializeField] private TextMeshProUGUI imposterAmountText;

    [SerializeField] private Button createLobbyButton;

    private int playerCount = OneInside.Constants.Player.MIN_PLAYERS;
    private int imposterCount = OneInside.Constants.Player.MIN_IMPOSTERS;

    private void Start()
    {
        UpdateCountTexts();
        SetupButtons();
        UpdateCreateLobbyButtonState();
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
                UpdateCreateLobbyButtonState();
            }
        });

        increasePlayerAmount.onClick.AddListener(() =>
        {
            if (playerCount < OneInside.Constants.Player.MAX_PLAYERS)
            {
                playerCount++;
                UpdateCountTexts();
                UpdateCreateLobbyButtonState();
            }
        });

        decreaseImposterAmount.onClick.AddListener(() =>
        {
            if (imposterCount > OneInside.Constants.Player.MIN_IMPOSTERS)
            {
                imposterCount--;
                UpdateCountTexts();
                UpdateCreateLobbyButtonState();
            }
        });

        increaseImposterAmount.onClick.AddListener(() =>
        {
            if (imposterCount < OneInside.Constants.Player.MAX_IMPOSTERS && imposterCount < playerCount - 1)
            {
                imposterCount++;
                UpdateCountTexts();
                UpdateCreateLobbyButtonState();
            }
        });

        createLobbyButton.onClick.AddListener(HostMatch);
    }

    private void UpdateCountTexts()
    {
        playerAmountText.text = playerCount.ToString();
        imposterAmountText.text = imposterCount.ToString();
    }

    private void UpdateCreateLobbyButtonState()
    {
        bool isValid = imposterCount >= OneInside.Constants.Player.MIN_IMPOSTERS &&
                      imposterCount <= OneInside.Constants.Player.MAX_IMPOSTERS &&
                      imposterCount < playerCount &&
                      playerCount >= OneInside.Constants.Player.MIN_PLAYERS &&
                      playerCount <= OneInside.Constants.Player.MAX_PLAYERS;

        createLobbyButton.interactable = isValid;
    }

    private async void HostMatch()
    {
        if (string.IsNullOrEmpty(lobbyNameInputField.text))
        {
            OneInsideGameManager.Instance.ShowMessage("Please enter a valid lobby name");
            return;
        }

        if (imposterCount < OneInside.Constants.Player.MIN_IMPOSTERS ||
            imposterCount > OneInside.Constants.Player.MAX_IMPOSTERS ||
            imposterCount >= playerCount ||
            playerCount < OneInside.Constants.Player.MIN_PLAYERS ||
            playerCount > OneInside.Constants.Player.MAX_PLAYERS)
        {
            OneInsideGameManager.Instance.ShowMessage("Invalid player or imposter count");
            return;
        }

        var createLobbyDto = new CreateLobbyDto(
            lobbyNameInputField.text,
            (GameMode)gameModeDropdown.value,
            playerCount,
            imposterCount,
            isPrivateToggle.isOn
        );

        if(OneInsideGameManager.Instance == null)
        {
            Debug.LogWarning("OneInsideGameManager is null!");
            return;
        }
        await OneInsideGameManager.Instance.HostMatch(createLobbyDto);
    }
}