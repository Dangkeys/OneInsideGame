using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public event Action OnSetAllPlayersToSpawnPos;
    public event Action<bool> OnEnableAllPlayersMovement;

    public override void OnNetworkSpawn()
    {
        if (!OneInsideLevelManager.Instance)
            return;
        OneInsideLevelManager.Instance.VoteManager.OnStateChanged += OnVoteStateChangedServerRPC;
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnVoteStateChangedServerRPC(VoteManager.State state)
    {

        OnVoteStateChangedClientRPC(state);
    }
    [ClientRpc]
    private void OnVoteStateChangedClientRPC(VoteManager.State state)
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
        OneInsideLevelManager.Instance.VoteManager.OnStateChanged -= OnVoteStateChangedServerRPC;
    }
}