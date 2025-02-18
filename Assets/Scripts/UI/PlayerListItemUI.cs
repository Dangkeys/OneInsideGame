using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItemUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerReadyImage;

    public void Initilize(string playerName, bool isReady) {
        playerNameText.text = playerName;
        playerReadyImage.enabled = isReady;
    }
}