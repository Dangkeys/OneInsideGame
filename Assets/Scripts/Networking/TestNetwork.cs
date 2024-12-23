using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestNetwork : MonoBehaviour
{   
    [field: SerializeField] public Button StartHostButton { get; private set; }
    [field: SerializeField] public TMP_InputField JoinCodeField { get; private set; }
    [field: SerializeField] public Button StartClientButton {get; private set;}
    private void Start()
    {
        StartHostButton.onClick.AddListener(StartHost);
        StartClientButton.onClick.AddListener(StartClient);
    }
    private async void StartHost()
    {
        await HostSingleton.Instance.GameManager.StartHostAsync();
    }
    public async void StartClient()
    {
        await ClientSingleton.Instance.GameManager.StartClientAsync(JoinCodeField.text);
    }

}
