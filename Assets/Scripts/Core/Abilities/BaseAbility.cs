using Unity.Netcode;
using UnityEngine;
using System;

public abstract class BaseAbility : NetworkBehaviour, IAbility
{
    [Header("Ability Settings")]
    [field: SerializeField] public float CooldownTime { get; private set; } = 30f;
    [field: SerializeField] public float ActiveTime { get; private set; } = 10f;

    public readonly NetworkVariable<AbilityState> State = new(AbilityState.READY);
    private InputReader inputReader;

    public bool IsReady => State.Value == AbilityState.READY;
    public NetworkVariable<float> RemainingCooldown { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> RemainingActiveTime { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        State.OnValueChanged += StateChanged;
        inputReader = OneInsideGameManager.Instance.InputReader;
        if (!IsOwner)
            return;
        inputReader.ActivateAbilityEvent += ActivateAbility;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        switch (State.Value)
        {
            case AbilityState.ACTIVE:
                UpdateActiveState();
                break;
            case AbilityState.COOLDOWN:
                UpdateCooldownState();
                break;
        }
    }

    private void UpdateActiveState()
    {
        if (RemainingActiveTime.Value <= 0)
        {
            SetStateServerRpc(AbilityState.COOLDOWN);
            return;
        }

        RemainingActiveTime.Value -= Time.deltaTime;
    }

    private void UpdateCooldownState()
    {
        if (RemainingCooldown.Value <= 0)
        {
            SetStateServerRpc(AbilityState.READY);
            return;
        }

        RemainingCooldown.Value -= Time.deltaTime;
    }

    private void ActivateAbility()
    {
        if (!CanActivate())
            return;
        if (IsReady)
        {
            SetStateServerRpc(AbilityState.ACTIVE);
        }
    }

    private void StateChanged(AbilityState previousValue, AbilityState newValue)
    {
        switch (newValue)
        {
            case AbilityState.READY:
                if (IsServer)
                {
                    OnReady();
                }
                break;
            case AbilityState.ACTIVE:
                if (IsServer)
                {
                    RemainingActiveTime.Value = ActiveTime;
                }
                OnActive();
                break;
            case AbilityState.COOLDOWN:
                if (IsServer)
                {
                    RemainingCooldown.Value = CooldownTime;
                }
                OnCooldown();
                break;
        }
    }

    protected virtual void OnReady()
    {
        RemainingCooldown.Value = 0;
        RemainingActiveTime.Value = 0;
    }

    protected virtual void OnActive()
    {
        Activate();
    }

    protected virtual void OnCooldown()
    {
        DeActivate();
    }

    public abstract void Activate();
    public abstract void DeActivate();

    [ServerRpc(RequireOwnership = false)]
    private void SetStateServerRpc(AbilityState newState)
    {
        if (!IsValidStateTransition(newState))
            return;
        State.Value = newState;
    }

    private bool IsValidStateTransition(AbilityState newState)
    {
        return (State.Value, newState) switch
        {
            (AbilityState.READY, AbilityState.ACTIVE) => true,
            (AbilityState.ACTIVE, AbilityState.COOLDOWN) => true,
            (AbilityState.COOLDOWN, AbilityState.READY) => true,
            _ => false
        };
    }

    public void UnSubScribeEvent()
    {
        State.OnValueChanged -= StateChanged;
        if (!IsOwner)
            return;
        inputReader.ActivateAbilityEvent -= ActivateAbility;
    }

    public abstract bool CanActivate();
}