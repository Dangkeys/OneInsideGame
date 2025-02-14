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

        createLobbyButton.onClick.AddListener(HostMatch);
    }

    private void UpdateCountTexts()
    {
        playerAmountText.text = playerCount.ToString();
        imposterAmountText.text = imposterCount.ToString();
    }

    private async void HostMatch()
    {
        if (string.IsNullOrEmpty(lobbyNameInputField.text))
        {
            Debug.LogWarning("Lobby name cannot be empty!");
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