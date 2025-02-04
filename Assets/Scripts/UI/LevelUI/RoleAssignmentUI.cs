
using System;
using TMPro;
using Unity.Netcode;

using UnityEngine;

public class RoleAssignemntUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roleText;
    private void Show(PlayerRole role)
    {
        roleText.text = $"YOUR ROLE IS {role.ToString().ToUpper()}";
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        if (OneInsideLevelManager.Instance == null)
        {
            Debug.LogError("OneInsideLevelManager.Instance is null");
            return;
        }
        OneInsideLevelManager.Instance.PlayerManager.OnAllPlayersInTheGame += HandleAllPlayersInTheGame;
        Hide();
    }

    private void HandleAllPlayersInTheGame(){
        SubscribeToRoleChangedEvent();
    }

    private void SubscribeToRoleChangedEvent()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.PlayerObject.TryGetComponent<Player>(out var player))
            {
                if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    player.Role.OnValueChanged += RoleChanged;
                }
            }
        }
    }
    private void UnSubscribeToRoleChangedEvent()
    {
        if (NetworkManager.Singleton == null)
            return;
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.PlayerObject.TryGetComponent<Player>(out var player))
            {
                if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    player.Role.OnValueChanged -= RoleChanged;
                }
            }
        }
    }

    private void RoleChanged(PlayerRole previousValue, PlayerRole newValue)
    {
        Show(newValue);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        OneInsideLevelManager.Instance.PlayerManager.OnAllPlayersInTheGame -= HandleAllPlayersInTheGame;
        UnSubscribeToRoleChangedEvent();

    }
}