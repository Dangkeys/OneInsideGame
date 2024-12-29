using System;
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

    [SerializeField] private Slider slider;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            VotingTimer.Value = votingTimerMax;
        }
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        switch (newValue)
        {
            case State.WaitingToVote:
                break;
            case State.Voting:
                slider.gameObject.SetActive(true);
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
                            VotingTimer.Value = 0;

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
    public void RaiseVoteStartServerRPC()
    {
        RaiseVoteStart();
    }
    private void RaiseVoteStart()
    {
        if (state.Value != State.Voting)
            state.Value = State.Voting;
    }

}