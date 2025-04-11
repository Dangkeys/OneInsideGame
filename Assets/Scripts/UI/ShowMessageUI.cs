using System;
using TMPro;
using UnityEngine;

public class ShowMessageUI : MonoBehaviour
{
    OneInsideGameManager gameManager;
    LobbyManager lobbyManager;
    [SerializeField] private TextMeshProUGUI messageText;
    void Start()
    {
        gameManager = OneInsideGameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("Failed to find OneInsideGameManager instance");
            return;
        }

        lobbyManager = gameManager.LobbyManager;
        if (lobbyManager == null)
        {
            Debug.LogError("LobbyManager is null");
            return;
        }

        lobbyManager.OnRequestFailed += ShowMessage;
        gameManager.UIManager.OnShowMessageRequired += ShowMessage;
        gameObject.SetActive(false);
    }

    private void ShowMessage(string obj)
    {
        messageText.text = obj;
        gameObject.SetActive(true);
    }

    private void ShowMessage(RequestErrorDto dto)
    {
        messageText.text = $"Error {dto.StatusCode}: {dto.Message}";
        gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        if (lobbyManager != null)
        {
            lobbyManager.OnRequestFailed -= ShowMessage;
        }
        if (gameManager != null)
        {
            gameManager.UIManager.OnShowMessageRequired -= ShowMessage;
        }
    }
}
