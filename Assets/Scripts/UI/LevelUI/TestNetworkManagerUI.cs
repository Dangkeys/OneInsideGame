using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using QFSW.QC;
using UnityEngine.SceneManagement;
using System;

public class TestNetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private Button startGameButton;

    private List<Button> startButtons;
    private bool isServer;
    private bool isClient;
    private ulong localClientId;

    private void Awake()
    {
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

    private void Start()
    {
        InitializeButtons();
        SetupNetworkCallbacks();

        // Initial UI state
        ShowStartButtons(true);
        disconnectButton.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);

    }

    private void InitializeButtons()
    {
        startButtons = new List<Button> { hostButton, clientButton, serverButton };

        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
        serverButton.onClick.AddListener(StartServer);
        disconnectButton.onClick.AddListener(Disconnect);
        startGameButton.onClick.AddListener(() =>
        {
            if (OneInsideLevelManager.Instance != null && OneInsideGameManager.Instance != null)
            {

                OneInsideLevelManager.Instance.SetGameState(GameState.GamePlaying);
            }
            else
            {
                Debug.Log("Can not start game, Please make sure OneInsideLevelManager and GameManager is present in the scene");
            }
            {
                startGameButton.gameObject.SetActive(false);
            }
        });
    }

    private void SetupNetworkCallbacks()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoadComplete;
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        }
    }

    private void OnServerStarted()
    {
        isServer = true;
        ShowStartButtons(false);
        disconnectButton.gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(true);  // Show start game when server/host starts
    }

    private void OnServerStopped(bool _)
    {
        isServer = false;
        if (!isClient)
        {
            ShowStartButtons(true);
            disconnectButton.gameObject.SetActive(false);
            startGameButton.gameObject.SetActive(false);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            localClientId = clientId;
            isClient = true;
            ShowStartButtons(false);
            disconnectButton.gameObject.SetActive(true);
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientId == localClientId)
        {
            isClient = false;
            if (!isServer)
            {
                ShowStartButtons(true);
                disconnectButton.gameObject.SetActive(false);
                startGameButton.gameObject.SetActive(false);
            }
        }
    }

    [Command]
    private void StartHost() => NetworkManager.Singleton.StartHost();

    [Command]
    private void StartClient() => NetworkManager.Singleton.StartClient();

    [Command]
    private void StartServer() => NetworkManager.Singleton.StartServer();

    [Command]
    private void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
        isServer = false;
        isClient = false;
        localClientId = 0;
        ShowStartButtons(true);
        disconnectButton.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);
    }

    private void ShowStartButtons(bool show)
    {
        foreach (Button button in startButtons)
        {
            button.gameObject.SetActive(show);
        }
    }
}
