using Unity.Netcode;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
     [Header("References")]
     [field: SerializeField] public GameObject PlayerVisual { get; private set; }

     private Animator playerAnimator;
     private Rigidbody[] ragdollRigidbodies;
     private Collider[] ragdollColliders;
     private PlayerMovement playerMovement;
     private CharacterController characterController;

     public NetworkVariable<bool> IsRagdollEnabled = new NetworkVariable<bool>(false,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server);

     public override void OnNetworkSpawn()
     {
          playerAnimator = GetComponent<Animator>();

          if (PlayerVisual != null)
          {
               ragdollColliders = PlayerVisual.GetComponentsInChildren<Collider>();
               ragdollRigidbodies = PlayerVisual.GetComponentsInChildren<Rigidbody>();
          }

          playerMovement = GetComponent<PlayerMovement>();
          characterController = GetComponent<CharacterController>();

          IsRagdollEnabled.OnValueChanged += On_Update_Ragdoll;
          On_Update_Ragdoll(false, false);
     }

     public override void OnNetworkDespawn()
     {
          IsRagdollEnabled.OnValueChanged -= On_Update_Ragdoll;
     }

     private void On_Update_Ragdoll(bool previousValue, bool newValue)
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

     [ServerRpc(RequireOwnership = false)]
     public void Set_Ragdoll_ServerRpc(bool value = true)
     {
          IsRagdollEnabled.Value = value;
     }

     private void EnableRagdoll()
     {
          playerAnimator.enabled = false;
          characterController.enabled = false;

          foreach (var rb in ragdollRigidbodies)
          {
               rb.isKinematic = false;
               rb.useGravity = true;
          }

          foreach (var col in ragdollColliders)
          {
               col.enabled = true;
          }
     }

     private void DisableRagdoll()
     {
          playerAnimator.enabled = true;
          characterController.enabled = true;

          foreach (var rb in ragdollRigidbodies)
          {
               rb.isKinematic = true;
               rb.useGravity = false;
          }

          foreach (var col in ragdollColliders)
          {
               col.enabled = false;
          }
     }

     public void TriggerAttackAnimation()
     {
          playerAnimator.SetTrigger("Attack");
     }

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
