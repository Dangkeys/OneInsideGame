using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OneInsideLevelManager : NetworkBehaviour
{


    [field: SerializeField] public PlayerManager PlayerManager;
    [field: SerializeField] public VoteManager VoteManager;

    [field: SerializeField] public RoleManager RoleManager;

    public event Action OnGameStart;

    public static OneInsideLevelManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadComplete;
    }

    private void OnSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != "TajdangScene")
            return;
        OnGameStart?.Invoke();
    }

    public void StartGame()
    {
        if(!IsClient && !IsHost)
        {
            OnGameStart?.Invoke();
        }
        StartGameClientRpc();
    }
    [ClientRpc]
    private void StartGameClientRpc()
    {
       OnGameStart?.Invoke();
    }
}