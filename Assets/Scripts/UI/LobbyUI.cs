using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button leaveMatchButton;
    [SerializeField] private Button updateLobbyButton;
    [SerializeField] private TextMeshProUGUI joinCodeText;
    void Start()
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            updateLobbyButton.gameObject.SetActive(false);
        }

        leaveMatchButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowConfirmation("Are you sure you want to leave the match?", async () => 
            {
                UIManager.Instance.ShowProgressChanged(.5f, "Leaving Match");
                await OneInsideGameManager.Instance.LeaveMatchAsync();
            }, null);
        });
        joinCodeText.text= $"Join Code: {LobbyManager.Instance.CurrentLobby.LobbyCode}";
    }
    // Update is called once per frame
    void Update()
    {

    }
}
