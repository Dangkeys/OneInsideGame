using Unity.Netcode;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    [Header("References")]
    [field: SerializeField] public GameObject PlayerVisual { get; private set; }
    [field: SerializeField] public Player PlayerScript { get; private set; }
    [field: SerializeField] public GameObject DeadbodyPrefab { get; private set; }

    //--------------------------------------
    // Private Variables
    //--------------------------------------
    private Animator playerAnimator;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private CharacterController characterController;
    private PlayerState playerState;
    private GameObject currentDeadbody;

    //--------------------------------------
    // Network & Lifecycle Methods
    //--------------------------------------
    public override void OnNetworkSpawn()
    {
        playerAnimator = GetComponent<Animator>();

        characterController = GetComponent<CharacterController>();

        ragdollColliders = PlayerVisual.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = PlayerVisual.GetComponentsInChildren<Rigidbody>();

        playerState = GetComponent<PlayerState>();
        playerState.Attacking.OnValueChanged += OnAttackingChanged;
        playerState.Stunning.OnValueChanged += OnStunningChanged;
        playerState.Walking.OnValueChanged += OnWalkingChanged;
        playerState.Running.OnValueChanged += OnRunningChanged;
        playerState.Ragdoll.OnValueChanged += OnRagdollChanged;

        DisableRagdoll();
    }

    public override void OnNetworkDespawn()
    {
        playerState.Attacking.OnValueChanged -= OnAttackingChanged;
        playerState.Stunning.OnValueChanged -= OnStunningChanged;
        playerState.Walking.OnValueChanged -= OnWalkingChanged;
        playerState.Running.OnValueChanged -= OnRunningChanged;
        playerState.Ragdoll.OnValueChanged -= OnRagdollChanged;
    }

    //--------------------------------------
    // State Change Handlers
    //--------------------------------------
    private void OnAttackingChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            playerAnimator.SetTrigger("Attack");
        }
    }

    private void OnStunningChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            playerAnimator.SetTrigger("Stun");
        }
    }

    private void OnWalkingChanged(bool previousValue, bool newValue)
    {
        playerAnimator.SetBool("Walking", newValue);
    }

    private void OnRunningChanged(bool previousValue, bool newValue)
    {
        playerAnimator.SetBool("Running", newValue);
    }

    private void OnRagdollChanged(bool previousValue, bool newValue)
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
    // Ragdoll Methods
    //--------------------------------------
    private void EnableRagdoll()
    {
        // playerAnimator.enabled = false;
        // characterController.enabled = false;

        // foreach (var rb in ragdollRigidbodies)
        // {
        //     rb.isKinematic = false;
        //     rb.useGravity = true;
        // }

        // foreach (var col in ragdollColliders)
        // {
        //     col.enabled = true;
        // }

        if (!IsOwner)
            return;

        DisableRagdoll();
        currentDeadbody = Instantiate(DeadbodyPrefab, transform.position, transform.rotation);
        currentDeadbody.GetComponent<NetworkObject>().Spawn();
    }

    private void DisableRagdoll()
    {
        // playerAnimator.enabled = true;
        // characterController.enabled = true;

        // foreach (var rb in ragdollRigidbodies)
        // {
        //     rb.isKinematic = true;
        //     rb.useGravity = false;
        // }

        // foreach (var col in ragdollColliders)
        // {
        //     col.enabled = false;
        // }

        if (!IsOwner)
            return;

        if (currentDeadbody)
        {
            currentDeadbody.GetComponent<NetworkObject>().Despawn();
        }
        currentDeadbody = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void Set_Dead_ServerRpc(bool value = true)
    {
        playerState.SetDeadServerRpc(value);
    }

    //--------------------------------------
    // Network Methods
    //--------------------------------------
    [ClientRpc]
    public void TriggerStunAnimationClientRpc()
    {
        TriggerStunAnimation();
    }

    public void TriggerStunAnimation()
    {
        playerAnimator.SetTrigger("Stun");
    }
}
