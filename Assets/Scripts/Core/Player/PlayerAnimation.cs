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
    private PlayerState playerState;
    private GameObject currentDeadbody;

    //--------------------------------------
    // Network & Lifecycle Methods
    //--------------------------------------
    public override void OnNetworkSpawn()
    {
        playerAnimator = GetComponent<Animator>();

        playerState = GetComponent<PlayerState>();
        playerState.Attacking.OnValueChanged += OnAttackingChanged;
        playerState.Stunning.OnValueChanged += OnStunningChanged;
        playerState.Walking.OnValueChanged += OnWalkingChanged;
        playerState.Running.OnValueChanged += OnRunningChanged;
        PlayerScript.IsAlive.OnValueChanged += OnAliveChanged;

        DisableRagdoll();
    }

    public override void OnNetworkDespawn()
    {
        playerState.Attacking.OnValueChanged -= OnAttackingChanged;
        playerState.Stunning.OnValueChanged -= OnStunningChanged;
        playerState.Walking.OnValueChanged -= OnWalkingChanged;
        playerState.Running.OnValueChanged -= OnRunningChanged;
        PlayerScript.IsAlive.OnValueChanged -= OnAliveChanged;
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

    private void OnAliveChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            DisableRagdoll();
        }
        else
        {
            EnableRagdoll();
        }
    }

    //--------------------------------------
    // Ragdoll Methods
    //--------------------------------------

    private void EnableRagdoll()
    {
        if (!IsLocalPlayer)
        {
            if (!PlayerManager.GetLocalPlayerScript().IsAlive.Value)
            {
                gameObject.SetActive(false);
            }
        }

        if (!IsServer)
            return;

        DisableRagdoll();

        currentDeadbody = Instantiate(DeadbodyPrefab, transform.position, transform.rotation);
        NetworkObject deadbodyNetworkObject = currentDeadbody.GetComponent<NetworkObject>();

        DeadBody deadBodyInteract = currentDeadbody.GetComponent<DeadBody>();
        deadBodyInteract.DeadBodyOwnerID.Value = PlayerScript.OwnerClientId;

        deadbodyNetworkObject.Spawn();

        if (OneInsideLevelManager.Instance)
        {
            deadbodyNetworkObject.TrySetParent(OneInsideLevelManager.DeadBodies, true);
        }

    }

    private void DisableRagdoll()
    {
        if (!IsServer)
            return;

        if (currentDeadbody)
        {
            currentDeadbody.GetComponent<NetworkObject>().Despawn();
        }
        currentDeadbody = null;
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
