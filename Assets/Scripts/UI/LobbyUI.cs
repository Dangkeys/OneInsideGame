using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button leaveMatchButton;

    void Start()
    {
        leaveMatchButton.onClick.AddListener(() =>
        {
            OneInsideGameManager.Instance.ShowConfirmation("Are you sure you want to leave the match?", async () => 
            {
                OneInsideGameManager.Instance.ShowProgressChanged(.5f, "Leaving Match");
                await OneInsideGameManager.Instance.LeaveMatchAsync();
            }, null);
        });
    }
    // Update is called once per frame
    void Update()
    {

    }
}
