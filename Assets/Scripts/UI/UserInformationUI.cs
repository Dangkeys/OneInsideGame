using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class UserInformationUI : MonoBehaviour
{
    private TextMeshProUGUI playerNameText;
    private Image playerAvatarImage;

    private void Awake()
    {
        playerNameText = GetComponentInChildren<TextMeshProUGUI>();
        playerAvatarImage = GetComponentInChildren<Image>();
    }

    private void Start()
    {
        if(!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("UserInformationUI: User is not signed in");
            return;
        }
        playerNameText.text = AuthenticationService.Instance.PlayerName;

        //TODO: Load player avatar
    }

}
