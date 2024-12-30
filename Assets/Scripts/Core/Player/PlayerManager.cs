using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public event Action OnSetAllPlayersToSpawnPos;
    public event Action<bool> OnEnableAllPlayersMovement;

    public List<Player> Players = new List<Player>();

    public override void OnNetworkSpawn()
    {
        if (!OneInsideLevelManager.Instance)
            return;
        NetworkManager.Singleton.OnConnectionEvent += NetworkOnConnectionEvent;
        OneInsideLevelManager.Instance.VoteManager.OnStateChanged += OnVoteStateChangedServerRpc;
    }

    private void NetworkOnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnVoteStateChangedServerRpc(VoteManager.State state)
    {

        OnVoteStateChangedClientRpc(state);
    }
    [ClientRpc]
    private void OnVoteStateChangedClientRpc(VoteManager.State state)
    {
        switch (state)
        {
            case VoteManager.State.WaitingToVote:
                break;
            case VoteManager.State.Voting:
                OnEnableAllPlayersMovement(false);
                OnSetAllPlayersToSpawnPos();
                break;
            case VoteManager.State.VoteOver:
                OnEnableAllPlayersMovement(true);
                break;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (!OneInsideLevelManager.Instance)
            return;
        OneInsideLevelManager.Instance.VoteManager.OnStateChanged -= OnVoteStateChangedServerRpc;
    }
}