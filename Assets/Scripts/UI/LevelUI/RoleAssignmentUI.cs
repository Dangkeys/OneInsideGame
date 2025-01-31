
using TMPro;
using Unity.Netcode;

using UnityEngine;

public class RoleAssignemntUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roleText;
    private void Show(Player.Role role)
    {
        roleText.text = $"YOUR ROLE IS {role.ToString().ToUpper()}";
        gameObject.SetActive(true);
    }

    private void Start()
    {
        if (OneInsideLevelManager.Instance == null)
        {
            Debug.LogError("OneInsideLevelManager.Instance is null");
            return;
        }
        OneInsideLevelManager.Instance.OnGameStart += HandleGameStart;
        Hide();
    }

    private void HandleGameStart()
    {
        Debug.Log("Game Started");
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
                    player.PlayerRole.OnValueChanged += RoleChanged;
                }
            }
        }
    }
    private void UnSubscribeToRoleChangedEvent()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.PlayerObject.TryGetComponent<Player>(out var player))
            {
                if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    player.PlayerRole.OnValueChanged -= RoleChanged;
                }
            }
        }
    }

    private void RoleChanged(Player.Role previousValue, Player.Role newValue)
    {
        Show(newValue);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        OneInsideLevelManager.Instance.OnGameStart -= HandleGameStart;
        UnSubscribeToRoleChangedEvent();

    }
}