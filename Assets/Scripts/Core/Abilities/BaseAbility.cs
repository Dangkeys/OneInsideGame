using Unity.Netcode;
using UnityEngine;

public enum AbilityState
{
    ReadyToActivate,
    Active,
    Cooldown
}

public abstract class BaseAbility : NetworkBehaviour
{
    [Header("Ability Settings")]
    [field:SerializeField] public float CooldownDuration {get; private set;} = 5f;
    [field:SerializeField] public float ActiveDuration {get; private set;} = 2f;
    [SerializeField] protected bool isPassiveAbility = false;
    [SerializeField] protected bool shouldStartWithCooldown = false;
    private InputReader inputReader;

    public NetworkVariable<AbilityState> CurrentState {get; private set;} = new NetworkVariable<AbilityState>(
        AbilityState.ReadyToActivate,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkTimer CooldownTimer { get; private set; }
    public NetworkTimer ActiveTimer { get; private set; }

    public bool IsPassive => isPassiveAbility;

    protected virtual void Awake()
    {
        if (!isPassiveAbility)
        {
            CooldownTimer = gameObject.AddComponent<NetworkTimer>();
            ActiveTimer = gameObject.AddComponent<NetworkTimer>();
        }
        inputReader = InputReader.Instance;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsPassive)
        {
            CurrentState.OnValueChanged += OnStateChanged;
        }

        if (IsServer)
        {
            if (isPassiveAbility)
            {
                CurrentState.Value = AbilityState.Active;
                return;
            }
            if (shouldStartWithCooldown)
            {
                CurrentState.Value = AbilityState.Cooldown;
            }
        }

        if (IsOwner)
        {
            inputReader.UseAbilityEvent += IsServer ? TryActivateAbility : TryActivateAbilityServerRpc;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsPassive)
        {
            CurrentState.OnValueChanged -= OnStateChanged;
        }
        if (IsOwner)
        {
            inputReader.UseAbilityEvent -= IsServer ? TryActivateAbility : TryActivateAbilityServerRpc;
        }
        base.OnNetworkDespawn();
    }

    protected virtual void OnStateChanged(AbilityState previousState, AbilityState newState)
    {
        if (isPassiveAbility)
            return;
        switch (newState)
        {
            case AbilityState.Active:
                OnAbilityActivated();
                break;
            case AbilityState.Cooldown:
                OnAbilityCooldownStarted();
                break;
            case AbilityState.ReadyToActivate:
                OnAbilityReady();
                break;
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void TryActivateAbilityServerRpc()
    {
        TryActivateAbility();
    }

    protected virtual void TryActivateAbility()
    {
        if (!IsServer || isPassiveAbility || CurrentState.Value != AbilityState.ReadyToActivate)
            return;

        CurrentState.Value = AbilityState.Active;
        ActiveTimer.StartTimer(ActiveDuration, () => StartCooldown());
    }

    protected virtual void StartCooldown()
    {
        if (!IsServer || isPassiveAbility)
            return;

        CurrentState.Value = AbilityState.Cooldown;
        CooldownTimer.StartTimer(CooldownDuration, () => CurrentState.Value = AbilityState.ReadyToActivate);
    }

    protected virtual void OnAbilityActivated()
    {
        // Override in child classes to implement ability activation logic
    }

    protected virtual void OnAbilityCooldownStarted()
    {
        // Override in child classes to implement cooldown start logic
    }

    protected virtual void OnAbilityReady()
    {
        // Override in child classes to implement ready state logic
    }
}
