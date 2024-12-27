using System.Collections.Generic;
using Mono.CSharp;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Player : NetworkBehaviour
{
     [Header("References")]
     [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
     [field: SerializeField] public InputReader InputReader { get; private set; }
     [field: SerializeField] public Object Hitbox { get; private set; }
     [field: SerializeField] public Object CharacterRoot { get; private set; }

     [Header("Settings")]
     public NetworkVariable<FixedString64Bytes> PlayerUUID = new NetworkVariable<FixedString64Bytes>("Test");
     public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>("Unknown");
     public NetworkVariable<int> PlayerHealth = new NetworkVariable<int>(3);

     [Header("State")]
     public NetworkVariable<bool> Attacking = new NetworkVariable<bool>(false,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
     );
     public NetworkVariable<bool> Dead = new NetworkVariable<bool>(false,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
     );

     //--------------------------------------
     private Rigidbody[] _ragdollRigidbodies;
     private Collider[] _ragdoll_colliders;
     private PlayerMovement _playerMovement;
     private Animator _playerAnimator;
     private BoxCollider[] _hitboxes;
     private CharacterController Character_Controller;

     //--------------------------------------

     private void EnableRagdoll()
     {
          _playerAnimator.enabled = false;
          _playerMovement.enabled = false;

          Character_Controller.excludeLayers = ~0;

          foreach (Rigidbody rb in _ragdollRigidbodies)
          {
               if (rb != null) rb.isKinematic = false;
          }

          foreach (Collider col in _ragdoll_colliders)
          {
               if (col != null) col.enabled = true;
          }
     }

     private void DisableRagdoll()
     {
          _playerAnimator.enabled = true;
          _playerMovement.enabled = true;

          Character_Controller.excludeLayers = 0;

          foreach (Rigidbody rb in _ragdollRigidbodies)
          {
               if (rb != null) rb.isKinematic = true;
          }

          foreach (Collider col in _ragdoll_colliders)
          {
               if (col != null) col.enabled = false;
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
          if (Attacking.Value || !_playerMovement.enabled) return;

          _playerAnimator.SetTrigger("Attack");

          Attack_ServerRpc(true);
          _playerMovement.enabled = false;

          var All_Hit_Characters = Az_Hitbox.Get_Touching_Objects(new Az_Hitbox.HitboxParams
          {
               Hitboxs = _hitboxes,
               Type = Az_Hitbox.Collider_Type.CharacterController,
               Exclude = new Collider[] { Character_Controller }
          });

          // get closest character
          GameObject Closest_Character = Az_Normal.Get_Closet_Target(transform.position, All_Hit_Characters);

          if (Closest_Character != null)
          {
               Debug.Log(Closest_Character.name);

               // Get the target's Player script or component and set Dead to true
               Player targetPlayer = Closest_Character.GetComponent<Player>();
               if (targetPlayer)
               {
                    // Mark the target as dead on the server
                    targetPlayer.Take_Damage_ServerRpc();
               }
          }

          await Awaitable.WaitForSecondsAsync(1);

          Attack_ServerRpc(false);

          if (Dead.Value) return;
          _playerMovement.enabled = true;
     }


     //--------------------------------------

     private void UpdatePlayerName(FixedString64Bytes oldValue, FixedString64Bytes newValue)
     {
          // Update the GameObject name on both server and clients
          gameObject.name = newValue.ToString();
     }

     //--------------------------------------

     public override void OnNetworkSpawn()
     {
          _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
          _playerMovement = GetComponent<PlayerMovement>();
          _playerAnimator = GetComponent<Animator>();
          Character_Controller = GetComponent<CharacterController>();
          _ragdoll_colliders = CharacterRoot.GetComponentsInChildren<Collider>();

          if (IsServer)
          {
               // Generate and set the PlayerUUID on the server
               PlayerUUID.Value = UUID.Create_ID();
          }

          PlayerUUID.OnValueChanged += UpdatePlayerName;
          UpdatePlayerName(PlayerUUID.Value, PlayerUUID.Value);

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
          PlayerUUID.OnValueChanged -= UpdatePlayerName;
          Dead.OnValueChanged -= UpdateDead;

          //----------------------
          if (!IsOwner) return;
          //----------------------
     }

     private void Update()
     {
          if (!IsOwner) return;

          //Debug
          if (Input.GetKeyDown(KeyCode.R))
          {
               Dead_ServerRpc(!Dead.Value);
          }
     }

     //Debug
     [ServerRpc(RequireOwnership = false)]
     public void Dead_ServerRpc(bool value = true)
     {
          Dead.Value = value;
          if (value == false)
          {
               PlayerHealth.Value = 3;
          }
     }

     [ServerRpc(RequireOwnership = false)]
     public void Take_Damage_ServerRpc(int Damage = 1)
     {
          ProcessDamageAsync(Damage);
     }

     private async void ProcessDamageAsync(int Damage = 1)
     {
          if (!_playerMovement.enabled) return;

          PlayerHealth.Value -= Damage;

          if (PlayerHealth.Value <= 0)
          {
               Dead_ServerRpc(true);
               return;
          }

          _playerAnimator.SetTrigger("Stun");

          _playerMovement.enabled = false;
          await Awaitable.WaitForSecondsAsync(2);

          if (Dead.Value) return;
          _playerMovement.enabled = true;
     }

     [ServerRpc(RequireOwnership = false)]
     public void Attack_ServerRpc(bool value = true)
     {
          Attacking.Value = value;
     }
}
