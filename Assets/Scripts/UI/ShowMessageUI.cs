using System;
using TMPro;
using UnityEngine;

public class ShowMessageUI : MonoBehaviour
{
    LobbyManager lobbyManager;

    UIManager uiManager;
    [SerializeField] private TextMeshProUGUI messageText;
    void Start()
    {


        lobbyManager = LobbyManager.Instance;
        uiManager = UIManager.Instance;
        if (lobbyManager == null)
        {
            Debug.LogError("LobbyManager is null");
            return;
        }

        lobbyManager.OnRequestFailed += ShowMessage;
        uiManager.OnShowMessageRequired += ShowMessage;
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

        uiManager.OnShowMessageRequired -= ShowMessage;

    }
}
