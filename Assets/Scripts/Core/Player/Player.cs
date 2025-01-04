using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
     [Header("References")]
     [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
     [field: SerializeField] public InputReader InputReader { get; private set; }
     [field: SerializeField] public GameObject Hitbox { get; private set; }
     [field: SerializeField] public Object CharacterRoot { get; private set; }
     [field: SerializeField] public PlayerAnimation PlayerAnimation { get; private set; }
     [field: SerializeField] public PlayerHealth PlayerHealth { get; private set; }
     [field: SerializeField] public CharacterController CharacterController { get; private set; }
     [field: SerializeField] public PlayerMovement PlayerMovement { get; private set; }
     [field: SerializeField] public CinemachineInputAxisController AxisController { get; private set; }

     [Header("Settings")]
     public NetworkVariable<FixedString64Bytes> PlayerID = new NetworkVariable<FixedString64Bytes>("Test");
     public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>("Unknown");

     [Header("State")]
     public NetworkVariable<bool> Attacking = new NetworkVariable<bool>(false,
          NetworkVariableReadPermission.Everyone,
          NetworkVariableWritePermission.Server
     );
     public NetworkVariable<bool> Stunning = new NetworkVariable<bool>(false,
          NetworkVariableReadPermission.Everyone,
          NetworkVariableWritePermission.Server
     );

     //--------------------------------------
     private BoxCollider[] hitBoxes;

     //--------------------------------------

     public async void On_Attack_Local()
     {
          if (!Is_Can_Move())
               return;

          Attack_ServerRpc(true);

          PlayerAnimation.TriggerAttackAnimation();

          // get closest character
          var All_Hit_Characters = Az_Hitbox.Get_Touching_Objects(new Az_Hitbox.HitboxParams
          {
               Hitboxs = hitBoxes,
               Type = Az_Hitbox.Collider_Type.CharacterController,
               Exclude = new Collider[] { CharacterController }
          });

          GameObject Closest_Character = Az_Normal.Get_Closet_Target(transform.position, All_Hit_Characters);

          if (Closest_Character != null)
          {
               Player targetPlayerScript = Closest_Character.GetComponent<Player>();
               if (targetPlayerScript)
               {
                    targetPlayerScript.Take_Damage_ServerRpc();
               }
          }

          await Awaitable.WaitForSecondsAsync(1);

          Attack_ServerRpc(false);
     }

     [ServerRpc(RequireOwnership = false)]
     public void Attack_ServerRpc(bool value = true)
     {
          Attacking.Value = value;
     }

     [ServerRpc(RequireOwnership = false)]
     private void Take_Damage_ServerRpc(int damage = 1)
     {
          Take_Damage(damage);
     }

     private async void Take_Damage(int damage = 1)
     {
          if (Stunning.Value)
               return;

          Stunning.Value = true;

          PlayerHealth.Take_Damage_ServerRpc(damage);

          await Awaitable.WaitForSecondsAsync(2.0f);

          Stunning.Value = false;
     }

     private bool Is_Can_Move()
     {
          return !(Stunning.Value || Attacking.Value || PlayerHealth.IsDead());
     }

     private void Update_Can_Move()
     {
          PlayerMovement.enabled = Is_Can_Move();
     }

     //--------------------------------------

     private void UpdatePlayerName(FixedString64Bytes oldValue, FixedString64Bytes newValue)
     {
          gameObject.name = newValue.ToString();
     }

     //--------------------------------------

     public override void OnNetworkSpawn()
     {
          CharacterController = GetComponent<CharacterController>();
          PlayerAnimation = GetComponent<PlayerAnimation>();

          if (IsServer)
          {
               PlayerName.Value = "Player " + NetworkObjectId;
          }

          PlayerID.OnValueChanged += UpdatePlayerName;
          UpdatePlayerName(PlayerID.Value, OwnerClientId.ToString());

          //----------------------

          hitBoxes = Hitbox.GetComponents<BoxCollider>();

          //----------------------
          if (!IsOwner)
          {
               // Other Player
               VirtualCamera.Priority = int.MinValue;
          }
          else
          {
               //----------------------

               InputReader.AttackEvent += On_Attack_Local;

               //----------------------

               Attacking.OnValueChanged += On_Update_Attacking;
               Stunning.OnValueChanged += On_Update_Stunning;
               PlayerHealth.Dead.OnValueChanged += On_Update_Dead;

               //----------------------

               OneInsideLevelManager.Instance.PlayerManager.OnSetAllPlayersToSpawnPos += ResetToSpawnPoint;
               OneInsideLevelManager.Instance.PlayerManager.OnEnableAllPlayersMovement += EnablePlayerMovement;
          }

     }

     public override void OnNetworkDespawn()
     {
          Attacking.OnValueChanged -= On_Update_Attacking;
          Stunning.OnValueChanged -= On_Update_Stunning;
          PlayerHealth.Dead.OnValueChanged -= On_Update_Dead;

          //----------------------
          if (!IsOwner)
               return;
          //----------------------

          if (!OneInsideLevelManager.Instance)
               return;
          OneInsideLevelManager.Instance.PlayerManager.OnSetAllPlayersToSpawnPos -= ResetToSpawnPoint;
     }

     private void On_Update_Dead(bool oldValue, bool newValue)
     {
          Update_Can_Move();
     }

     private void On_Update_Attacking(bool oldValue, bool newValue)
     {
          Update_Can_Move();
     }

     private void On_Update_Stunning(bool oldValue, bool newValue)
     {
          Update_Can_Move();
     }
     private void Update()
     {
          if (!IsOwner)
               return;

          //Debug
          if (Input.GetKeyDown(KeyCode.R))
          {
               PlayerHealth.Set_Health_ServerRpc(PlayerHealth.MaxHealth.Value);
          }
     }

     //--------------------------------------

     private void EnablePlayerMovement(bool shouldMove)
     {
          if (!shouldMove)
          {
               InputReader.DisableGameplayInput();

          }
          else
          {
               InputReader.EnableGameplayInput();
          }
          if (AxisController)
               AxisController.enabled = shouldMove;
     }
     public void ResetToSpawnPoint()
     {
          CharacterController.enabled = false;
          transform.position = SpawnPoint.GetClientSpawnPos(NetworkManager.Singleton.LocalClientId);
          CharacterController.enabled = true;
     }
}
