using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button quickPlayButton;
    [SerializeField] private Button exitButton;
    void Start()
    {
        quickPlayButton.onClick.AddListener(async ()=>{
            await OneInsideGameManager.Instance.QuickJoinMatchAsync();
        });

        exitButton.onClick.AddListener(()=>{
            OneInsideGameManager.Instance.ShowConfirmation("Are you sure you want to exit?", Application.Quit);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
