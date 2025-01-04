using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
     [Header("Health Settings")]
     public NetworkVariable<int> MaxHealth = new NetworkVariable<int>(3);

     public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
     public NetworkVariable<bool> Dead = new NetworkVariable<bool>(false,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server);

     private PlayerAnimation playerAnimation;

     public override void OnNetworkSpawn()
     {
          if (IsServer)
          {
               CurrentHealth.Value = MaxHealth.Value;
          }

          playerAnimation = GetComponent<PlayerAnimation>();

          Dead.OnValueChanged += UpdateDead;
     }

     [ServerRpc(RequireOwnership = false)]
     public void Take_Damage_ServerRpc(int damage = 1)
     {
          CurrentHealth.Value -= damage;
          playerAnimation.TriggerStunAnimationClientRpc();

          CheckDead();
     }

     [ServerRpc]
     public void Set_Health_ServerRpc(int amount = 1)
     {
          CurrentHealth.Value = amount;

          CheckDead();
     }

     [ServerRpc(RequireOwnership = false)]
     public void Respawn_ServerRpc()
     {
          if (!IsServer)
               return;

          CurrentHealth.Value = MaxHealth.Value;
          Dead.Value = false;
     }

     private void CheckDead()
     {
          if (CurrentHealth.Value <= 0)
          {
               Dead.Value = true;
          }
          else
          {
               Dead.Value = false;
          }
     }

     private void UpdateDead(bool oldValue, bool newValue)
     {
          if (!IsServer)
               return;

          playerAnimation.Set_Ragdoll_ServerRpc(newValue);
     }

     public bool IsDead()
     {
          return Dead.Value;
     }

     public override void OnNetworkDespawn()
     {
          Dead.OnValueChanged -= UpdateDead;
     }
}
