using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public event Action OnSetAllPlayersToSpawnPos;
    public static PlayerManager Instance { get; private set; }
    private void Start() {
        Instance = this;
    }
    [ServerRpc(RequireOwnership = false)]
    public void OnSetAllPlayersToSpawnPosServerRPC()
    {
        OnSetAllPlayersToSpawnPosClientRPC();
    }
    [ClientRpc]
    private void OnSetAllPlayersToSpawnPosClientRPC()
    {
        OnSetAllPlayersToSpawnPos?.Invoke();
    }


}