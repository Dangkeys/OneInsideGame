using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public event Action OnSetAllPlayersToSpawnPos;
    public event Action<bool> OnEnableAllPlayersMovement;

    public NetworkVariable<List<ulong>> PlayerClientIds = new NetworkVariable<List<ulong>>(new List<ulong>());

    public override void OnNetworkSpawn()
    {
        if (!OneInsideLevelManager.Instance)
            return;
        NetworkManager.Singleton.OnConnectionEvent += NetworkOnConnectionEvent;

        OneInsideLevelManager.Instance.VoteManager.OnStateChanged += OnVoteStateChangedServerRpc;
        PlayerClientIds.OnValueChanged += OnPlayerClientIdsChanged;
    }

    private void OnPlayerClientIdsChanged(List<ulong> previousValue, List<ulong> newValue)
    {
        Debug.Log(newValue);
    }

    private void NetworkOnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        //add player to list
        switch (data.EventType)
        {
            case ConnectionEvent.ClientConnected:
                if (IsServer)
                {
                    if (PlayerClientIds.Value.Contains(data.ClientId))
                    {
                        return;
                    }
                    PlayerClientIds.Value.Add(data.ClientId);
                }
                break;
            case ConnectionEvent.ClientDisconnected:
                if (IsServer)
                {
                    if (!PlayerClientIds.Value.Contains(data.ClientId))
                    {
                        return;
                    }
                    PlayerClientIds.Value.Remove(data.ClientId);
                }
                break;
        }
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