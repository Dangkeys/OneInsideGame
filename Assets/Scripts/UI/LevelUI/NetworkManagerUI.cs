using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using QFSW.QC;
using UnityEngine.SceneManagement;
using System;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private Button startGameButton;
    OneInsideGameManager oneInsideGameManager;
    private void Awake()
    {
        oneInsideGameManager = OneInsideGameManager.Instance;
        SceneManager.sceneLoaded += OnSceneLoadComplete;
    }

    private void OnSceneLoadComplete(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == GameScene.LobbyScene.ToString())
        {
            SceneManager.sceneLoaded -= OnSceneLoadComplete;
            Debug.Log("OnSceneLoadComplete - Disabling NetworkManagerUI");
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        startGameButton.gameObject.SetActive(false);
        disconnectButton.gameObject.SetActive(false);

        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
        serverButton.onClick.AddListener(StartServer);
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        if (IsServer)
        {
            startGameButton.gameObject.SetActive(true);

            startGameButton.onClick.AddListener(() =>
            {
                OneInsideLevelManager.Instance.SetGameState(GameState.GamePlaying);
                startGameButton.gameObject.SetActive(false);
            });

        }
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        serverButton.gameObject.SetActive(false);
        disconnectButton.onClick.AddListener(Disconnect);
        disconnectButton.gameObject.SetActive(true);
    }

    public override void OnNetworkDespawn()
    {
        startGameButton.gameObject.SetActive(false);
        disconnectButton.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(true);
        clientButton.gameObject.SetActive(true);
        serverButton.gameObject.SetActive(true);
    }


    [Command]
    private async void StartHost()
    {
        await oneInsideGameManager.InitializeGameAsync(shouldLoadScene: false);
        NetcodeManager.TransmitUserData();
        NetworkManager.Singleton.StartHost();
    }

    [Command]
    private async void StartClient()
    {
        await oneInsideGameManager.InitializeGameAsync(shouldLoadScene: false);
        NetcodeManager.TransmitUserData();
        NetworkManager.Singleton.StartClient();
    }

    [Command]
    private void StartServer() => NetworkManager.Singleton.StartServer();

    [Command]
    private void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }
}