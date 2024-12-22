using Mono.CSharp;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("References")]
    [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public Object Hitbox { get; private set; }

    [Header("Settings")]
    // public NetworkVariable<string> PlayerUUID = new NetworkVariable<string>(UUID.Create_ID(),
    //     NetworkVariableReadPermission.Everyone,
    //     NetworkVariableWritePermission.Server
    // );
    // public NetworkVariable<string> PlayerName = new NetworkVariable<string>("Unnamed Player",
    //     NetworkVariableReadPermission.Everyone,
    //     NetworkVariableWritePermission.Server
    // );

    [Header("State")]
    public NetworkVariable<bool> Attacking = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<bool> Dead = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Rigidbody[] _ragdollRigidbodies;
    private PlayerMovement _playerMovement;
    private Animator _playerAnimator;
    private BoxCollider[] _hitboxes;

    //--------------------------------------

    private void EnableRagdoll()
    {
        _playerAnimator.enabled = false;
        _playerMovement.enabled = false;

        foreach (Rigidbody rb in _ragdollRigidbodies)
        {
            if (rb != null) rb.isKinematic = false;
        }
    }

    private void DisableRagdoll()
    {
        _playerAnimator.enabled = true;
        _playerMovement.enabled = true;

        foreach (Rigidbody rb in _ragdollRigidbodies)
        {
            if (rb != null) rb.isKinematic = true;
        }
    }

    private void UpdateDead(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            EnableRagdoll();
        }
        else
        {
            DisableRagdoll();
        }
    }
    //--------------------------------------

    public async void On_Attack_Local()
    {
        if (Attacking.Value) return;

        _playerAnimator.SetTrigger("Attack");

        Attacking.Value = true;

        // Attack_ServerRpc(true);
        // _playerMovement.enabled = false;

        await Awaitable.WaitForSecondsAsync(1);

         Attacking.Value = false;

        // Attack_ServerRpc(false);
        // _playerMovement.enabled = true;
    }

    //--------------------------------------

    public override void OnNetworkSpawn()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAnimator = GetComponent<Animator>();

        Dead.OnValueChanged += UpdateDead;
        UpdateDead(false, Dead.Value);

        //----------------------

        _hitboxes = Hitbox.GetComponents<BoxCollider>();

        //----------------------
        if (!IsOwner)
        {
            // Other Player
            VirtualCamera.Priority = int.MinValue;
            return;
        }
        //----------------------

        Debug.Log($"IsServer: {IsServer}, IsClient: {IsClient}, IsOwner: {IsOwner}");
        InputReader.AttackEvent += On_Attack_Local;

        //----------------------


    }

    public override void OnNetworkDespawn()
    {
        Dead.OnValueChanged -= UpdateDead;

        //----------------------
        if (!IsOwner) return;
        //----------------------
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Dead_ServerRpc(Dead.Value = !Dead.Value);
        }
    }

    [ServerRpc]
    public void Dead_ServerRpc(bool value = true)
    {
        Dead.Value = value;
    }
    [ServerRpc(RequireOwnership = false)]
    public void Attack_ServerRpc(bool value = true)
    {
        Attacking.Value = value;

        if (!value) return;
        Attack_ClientRpc();
    }

    [ClientRpc]
    void Attack_ClientRpc()
    {
        Debug.Log(_playerAnimator);
        _playerAnimator.SetTrigger("Attack");

        // if (IsLocalPlayer)
        // {
        //     Debug.Log(_hitboxes);
        //     var touchingColliders = Az_Hitbox.Get_Touching_Colliders(_hitboxes, "Player");
        //     Debug.Log(touchingColliders);
        // }

    }
}
