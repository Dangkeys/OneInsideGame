using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuNavBar : MonoBehaviour
{
    [field: SerializeField] public Button CreateRoomButton { get; private set; }
    [field: SerializeField] public Button FindMatchButton { get; private set; }
    [field: SerializeField] public Button WaitingRoomButton { get; private set; }

    private void Show(Button button, bool shouldShow)
    {
        button.gameObject.SetActive(shouldShow);
    }
    private void Start()
    {
        MainMenuUI.Instance.OnLobbyValueChanged += OnLobbyValueChanged;
    }

    private void OnLobbyValueChanged(Lobby lobby)
    {
        if (lobby == null)
        {
            Show(CreateRoomButton, true);
            Show(FindMatchButton, true);
            Show(WaitingRoomButton, false);
        }
        else
        {
            Show(CreateRoomButton, false);
            Show(FindMatchButton, false);
            Show(WaitingRoomButton, true);
        }
    }
}
