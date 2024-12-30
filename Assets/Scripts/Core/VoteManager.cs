using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class VoteManager : NetworkBehaviour
{


    public enum State
    {
        WaitingToVote,
        Voting,
        VoteOver,
    }
    public event Action<State> OnStateChanged;


    [field: SerializeField] private float votingTimerMax = 60f;
    public NetworkVariable<float> VotingTimer = new NetworkVariable<float>();
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToVote);
    private NetworkVariable<Dictionary<ulong, ulong?>> voteDictionary = new NetworkVariable<Dictionary<ulong, ulong?>>();
    [SerializeField] private Slider slider;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            VotingTimer.Value = votingTimerMax;
        }
        state.OnValueChanged += StateChanged;
        voteDictionary.OnValueChanged += VoteDictionaryChanged;
    }

    private void VoteDictionaryChanged(Dictionary<ulong, ulong?> previousValue, Dictionary<ulong, ulong?> newValue) => throw new NotImplementedException();

    private void StateChanged(State previousValue, State newValue)
    {
        switch (newValue)
        {
            case State.WaitingToVote:
                break;
            case State.Voting:
                slider.gameObject.SetActive(true);
                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    voteDictionary.Value.Add(clientId, null);
                }
                break;
            case State.VoteOver:
                slider.gameObject.SetActive(false);
                break;
        }
        OnStateChanged?.Invoke(newValue);
    }


    private void Update()
    {
        switch (state.Value)
        {
            case State.WaitingToVote:
                break;
            case State.Voting:
                if (IsServer)
                {
                    if (VotingTimer.Value > 0)
                    {
                        VotingTimer.Value -= Time.deltaTime;

                        if (VotingTimer.Value < 0)
                        {
                            VotingTimer.Value = votingTimerMax;

                            state.Value = State.VoteOver;
                        }
                    }
                }

                if (slider != null)
                {
                    slider.value = VotingTimer.Value / votingTimerMax;
                }
                break;
            case State.VoteOver:
                break;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void RaiseVoteStartServerRpc()
    {
        RaiseVoteStart();
    }
    private void RaiseVoteStart()
    {
        if (state.Value != State.Voting)
            state.Value = State.Voting;
    }
}