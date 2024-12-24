using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestNetworkMangerUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button disconnectButton;
    private List<Button> startButtons = new List<Button>();

    private ulong localClientId;
    private bool isServer;
    private bool isClient;

    void Start()
    {
        ResetNetworkState();

        NetworkManager.Singleton.OnClientConnectedCallback += Network_ClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += Network_ClientDisconnect;
        NetworkManager.Singleton.OnServerStarted += Network_OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += Network_OnServerStopped;

        startButtons.Add(startServerButton);
        startButtons.Add(startClientButton);
        startButtons.Add(startHostButton);

        startHostButton.onClick.AddListener(StartHost);
        startClientButton.onClick.AddListener(StartClient);
        startServerButton.onClick.AddListener(StartServer);
        disconnectButton.onClick.AddListener(Disconnect);

        ShowStartButtons(true);
        disconnectButton.gameObject.SetActive(false);
    }

    private void Network_OnServerStarted()
    {
        isServer = true;
        ShowStartButtons(false);
        disconnectButton.gameObject.SetActive(true);
    }

    private void Network_OnServerStopped(bool obj)
    {
        isServer = false;
        if (!isClient)
        {
            ShowStartButtons(true);
            disconnectButton.gameObject.SetActive(false);
        }
    }

    private void Network_ClientDisconnect(ulong clientId)
    {
        if (clientId == localClientId)
        {
            isClient = false;
            if (!isServer)
            {
                ShowStartButtons(true);
                disconnectButton.gameObject.SetActive(false);
            }
        }
    }

    private void Network_ClientConnected(ulong clientId)
    {
        localClientId = NetworkManager.Singleton.LocalClientId;
        if (clientId == localClientId)
        {
            isClient = true;
            ShowStartButtons(false);
            disconnectButton.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // Failsafe check for disconnection
        if (NetworkManager.Singleton != null &&
            !NetworkManager.Singleton.IsConnectedClient &&
            !NetworkManager.Singleton.IsServer &&
            !isServer && !isClient)
        {
            ShowStartButtons(true);
            disconnectButton.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Network_ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Network_ClientDisconnect;
            NetworkManager.Singleton.OnServerStarted -= Network_OnServerStarted;
            NetworkManager.Singleton.OnServerStopped -= Network_OnServerStopped;
        }
    }

    [Command]
    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    [Command]
    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    [Command]
    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    [Command]
    private void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
        isServer = false;
        isClient = false;
        localClientId = 0;
        ShowStartButtons(true);
        disconnectButton.gameObject.SetActive(false);
    }

    private void ResetNetworkState()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            isServer = false;
            isClient = false;
            localClientId = 0;
            StartCoroutine(EnableStartButtonsNextFrame());
        }
    }

    private IEnumerator EnableStartButtonsNextFrame()
    {
        yield return new WaitForEndOfFrame();
        ShowStartButtons(true);
        disconnectButton.gameObject.SetActive(false);
    }

    private void ShowStartButtons(bool shouldShow)
    {
        foreach (Button button in startButtons)
        {
            button.gameObject.SetActive(shouldShow);
        }
    }
}