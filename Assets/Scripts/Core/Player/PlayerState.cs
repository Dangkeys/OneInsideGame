using System.Collections.Generic;
using OneInside.Constants;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    /*
    -------------------------------------------------------
    Player Identification
    -------------------------------------------------------
    */
    [Header("Info")]
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

    /*
    -------------------------------------------------------
    Player States
    -------------------------------------------------------
    */
    [Header("Status")]
    [field: SerializeField]
    public NetworkVariable<bool> Attacking { get; private set; } = new NetworkVariable<bool>(
         default,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Owner
    );
    [field: SerializeField]
    public NetworkVariable<bool> Poking { get; private set; } = new NetworkVariable<bool>(
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

    /*
    -------------------------------------------------------
    Health
    -------------------------------------------------------
    */
    [Header("Health")]
    [field: SerializeField]
    public NetworkVariable<float> CurrentHealth { get; private set; } = new NetworkVariable<float>(
         DefaultPlayerConfig.Player.MAX_HEALTH,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
    );
    [field: SerializeField]
    public NetworkVariable<float> MaxHealth { get; private set; } = new NetworkVariable<float>(
         DefaultPlayerConfig.Player.MAX_HEALTH,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
    );

    [Header("Dependencies")]
    [field: SerializeField] public PlayerMovement PlayerMovement { get; private set; }
    [field: SerializeField] public Player Player { get; private set; }

    /*
    -------------------------------------------------------
    Network & Lifecycle Methods
    -------------------------------------------------------
    */
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerName.Value = "Player " + NetworkObjectId;
            PlayerID.Value = OwnerClientId;
            MaxHealth.Value = DefaultPlayerConfig.Player.MAX_HEALTH;
            CurrentHealth.Value = MaxHealth.Value;
            Stunning.Value = false;
        }

        if (IsOwner)
        {
            Attacking.Value = false;
            Poking.Value = false;
            Walking.Value = false;
            Running.Value = false;
        }


        Attacking.OnValueChanged += UpdateCanMove;
        Stunning.OnValueChanged += UpdateCanMove;
        Player.IsAlive.OnValueChanged += UpdateCanMove;
    }

    public override void OnNetworkDespawn()
    {
        Attacking.OnValueChanged -= UpdateCanMove;
        Stunning.OnValueChanged -= UpdateCanMove;
        Player.IsAlive.OnValueChanged -= UpdateCanMove;
    }

    /*
    -------------------------------------------------------
    Health Management Methods
    -------------------------------------------------------
    */

    private void UpdateAlive()
    {
        Debug.Log(CurrentHealth.Value);
        SetAliveServerRpc(CurrentHealth.Value > 0);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeHealthServerRpc(float damage)
    {
        if (IsServer && Player.IsAlive.Value)
        {
            CurrentHealth.Value = Mathf.Max(CurrentHealth.Value - damage, 0);
            UpdateAlive();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(int healAmount)
    {
        if (IsServer)
        {
            CurrentHealth.Value = Mathf.Min(CurrentHealth.Value + healAmount, MaxHealth.Value);
            UpdateAlive();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHealthServerRpc(float amount)
    {
        if (IsServer)
        {
            CurrentHealth.Value = Mathf.Max(Mathf.Min(amount, MaxHealth.Value), 0);
            UpdateAlive();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAliveServerRpc(bool value)
    {
        SetAlive(value);
    }

    /*
    -------------------------------------------------------
    State Management Methods
    -------------------------------------------------------
    */
    public void SetAttacking(bool value)
    {
        if (IsOwner)
        {
            Attacking.Value = value;
        }
    }

    public void SetStunning(bool value)
    {
        if (IsServer)
        {
            Stunning.Value = value;
        }
    }

    public void SetWalking(bool value)
    {
        if (IsOwner)
        {
            Walking.Value = value;
        }
    }

    public void SetRunning(bool value)
    {
        if (IsOwner)
        {
            Running.Value = value;
        }
    }

    public void SetPoking(bool value)
    {
        if (IsOwner)
        {
            Poking.Value = value;
        }
    }

    public void SetAlive(bool value)
    {
        if (IsServer)
        {
            Player.IsAlive.Value = value;
        }
    }

    /*
    -------------------------------------------------------
    Movement Control Methods
    -------------------------------------------------------
    */
    public bool IsCanMove()
    {
        // Should have something that can make player can't move :D
        return true;
    }

    public bool IsCanJump()
    {
        return PlayerMovement.Behaviour != MovementBehaviour.STUNNING;
    }

    private void UpdateCanMove(bool previous, bool current)
    {
        if (PlayerMovement != null)
        {
            PlayerMovement.enabled = IsCanMove();
        }
    }

    /*
    -------------------------------------------------------
    Utility Methods
    -------------------------------------------------------
    */
    public float HealthPercentage => MaxHealth.Value > 0 ? (float)CurrentHealth.Value / MaxHealth.Value : 0;
}
