using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    [Header("Player Identification")]
    [field: SerializeField]
    public NetworkVariable<ulong> PlayerID { get; private set; } = new NetworkVariable<ulong>(
         default,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
    );
    [field: SerializeField]
    public NetworkVariable<FixedString64Bytes> PlayerName { get; private set; } = new NetworkVariable<FixedString64Bytes>(
         default,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
    );

    [Header("Player States")]
    [field: SerializeField]
    public NetworkVariable<bool> Attacking { get; private set; } = new NetworkVariable<bool>(
         default,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Owner
    );
    [field: SerializeField]
    public NetworkVariable<bool> Stunning { get; private set; } = new NetworkVariable<bool>(
         default,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
    );
    [field: SerializeField]
    public NetworkVariable<bool> Walking { get; private set; } = new NetworkVariable<bool>(
         default,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Owner
    );
    [field: SerializeField]
    public NetworkVariable<bool> Running { get; private set; } = new NetworkVariable<bool>(
         default,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Owner
    );
    [field: SerializeField]
    public NetworkVariable<bool> Ragdoll { get; private set; } = new NetworkVariable<bool>(
         default,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
    );

    [Header("Health")]
    [field: SerializeField]
    public NetworkVariable<int> CurrentHealth { get; private set; } = new NetworkVariable<int>(
         3,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
    );
    [field: SerializeField]
    public NetworkVariable<int> MaxHealth { get; private set; } = new NetworkVariable<int>(
         3,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
    );
    [field: SerializeField]
    public NetworkVariable<bool> Dead { get; private set; } = new NetworkVariable<bool>(
         false,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
    );

    [Header("Dependencies")]
    [field: SerializeField] public PlayerMovement PlayerMovement { get; private set; }

    //--------------------------------------
    // Network & Lifecycle Methods
    //--------------------------------------
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerName.Value = "Player " + NetworkObjectId;
            PlayerID.Value = OwnerClientId;
            CurrentHealth.Value = MaxHealth.Value;
        }

        Attacking.OnValueChanged += Update_Can_Move;
        Stunning.OnValueChanged += Update_Can_Move;
        Dead.OnValueChanged += Update_Can_Move;
    }

    public override void OnNetworkDespawn()
    {
        Attacking.OnValueChanged -= Update_Can_Move;
        Stunning.OnValueChanged -= Update_Can_Move;
        Dead.OnValueChanged -= Update_Can_Move;
    }

    //--------------------------------------
    // Health Management Methods
    //--------------------------------------
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (IsServer && !Dead.Value)
        {
            CurrentHealth.Value = Mathf.Max(CurrentHealth.Value - damage, 0);

            if (CurrentHealth.Value <= 0)
            {
                SetDeadServerRpc(true);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(int healAmount)
    {
        if (IsServer && !Dead.Value)
        {
            CurrentHealth.Value = Mathf.Min(CurrentHealth.Value + healAmount, MaxHealth.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHealthServerRpc(int amount)
    {
        if (IsServer)
        {
            CurrentHealth.Value = Mathf.Min(CurrentHealth.Value + amount, MaxHealth.Value);
            Dead.Value = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDeadServerRpc(bool value)
    {
        SetDead(value);
    }

    //--------------------------------------
    // State Management Methods
    //------------------------------
    public void SetAttacking(bool value)
    {
        if (IsOwner)
        { Attacking.Value = value; }
    }

    public void SetStunning(bool value)
    {
        if (IsServer)
        { Stunning.Value = value; }
    }

    public void SetWalking(bool value)
    {
        if (IsOwner)
        { Walking.Value = value; }
    }

    public void SetRunning(bool value)
    {
        if (IsOwner)
        { Running.Value = value; }
    }

    public void SetDead(bool value)
    {
        if (IsServer)
        {
            Dead.Value = value;
            Ragdoll.Value = value;
        }
    }

    //--------------------------------------
    // Movement Control Methods
    //--------------------------------------
    public bool Is_Can_Move()
    {
        return !(Stunning.Value || Attacking.Value || Dead.Value);
    }

    private void Update_Can_Move(bool previous, bool current)
    {
        PlayerMovement.enabled = Is_Can_Move();
    }

    //--------------------------------------
    // Utility Methods
    //--------------------------------------
    public float HealthPercentage => (float)CurrentHealth.Value / MaxHealth.Value;
}
