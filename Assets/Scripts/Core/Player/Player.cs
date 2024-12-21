using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
    public NetworkVariable<bool> Ragdoll = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Rigidbody[] _ragdollRigidbodies;
    private PlayerMovement _playerMovement;
    private Animator _playerAnimator;

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

    private void UpdateRagdoll(bool oldValue, bool newValue)
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

    public override void OnNetworkSpawn()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAnimator = GetComponent<Animator>();

        Ragdoll.OnValueChanged += UpdateRagdoll;
        UpdateRagdoll(false, Ragdoll.Value);

        if (!IsOwner)
        {
            VirtualCamera.Priority = int.MinValue;
        }
    }

    public override void OnNetworkDespawn()
    {
        Ragdoll.OnValueChanged -= UpdateRagdoll;

        if (!IsOwner) return;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRagdollServerRpc();
        }
    }

    [ServerRpc]
    public void ToggleRagdollServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Ragdoll.Value = !Ragdoll.Value;
    }
}
