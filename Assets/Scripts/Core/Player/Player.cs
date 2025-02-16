using System;
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
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public PlayerMovement PlayerMovement { get; private set; }

    public NetworkVariable<bool> IsAlive = new NetworkVariable<bool>(true);

    public NetworkVariable<PlayerRole> Role = new NetworkVariable<PlayerRole>(PlayerRole.None);

    //--------------------------------------
    // Private Variables
    //--------------------------------------

    private BoxCollider[] hitBoxes;
    private PlayerState playerState;

    public override void OnNetworkSpawn()
    {

        hitBoxes = Hitbox.GetComponents<BoxCollider>();

        playerState = GetComponent<PlayerState>();

        Role.OnValueChanged += OnRoleChanged;
        playerState.Spectator.OnValueChanged += OnSpectatorChanged;

        if (!IsOwner)
        {
            // Other Player
            VirtualCamera.Priority = int.MinValue;
        }
        else
        {
            InputReader.AttackEvent += On_Attack_Local;

            playerState.SetAttacking(false);
            playerState.SetStunning(false);
            playerState.SetWalking(false);
            playerState.SetRunning(false);
            playerState.SetDead(false);
            if (!OneInsideLevelManager.Instance)
                return;
            OneInsideLevelManager.Instance.PlayerManager.OnResetALlPlayerPosition += ResetToSpawnPoint;
        }
    }

    public override void OnNetworkDespawn()
    {
        Role.OnValueChanged -= OnRoleChanged;
        playerState.Spectator.OnValueChanged -= OnSpectatorChanged;

        if (!IsOwner)
            return;

        InputReader.AttackEvent -= On_Attack_Local;

        if (!OneInsideLevelManager.Instance)
            return;
        OneInsideLevelManager.Instance.PlayerManager.OnResetALlPlayerPosition -= ResetToSpawnPoint;

    }

    //--------------------------------------
    // Debug Methods
    //--------------------------------------

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            playerState.SetDeadServerRpc(false);
            playerState.SetHealthServerRpc(playerState.MaxHealth.Value);
        }
    }
    //--------------------------------------
    // Attack Methods
    //--------------------------------------

    public async void On_Attack_Local()
    {
        if (!playerState.Is_Can_Move())
            return;

        playerState.SetAttacking(true);

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
            PlayerState targetPlayerState = Closest_Character.GetComponent<PlayerState>();
            if (targetPlayerScript && targetPlayerState.Dead.Value == false && !PlayerManager_Local.GetLocalPlayer().GetComponent<PlayerState>().Dead.Value)
            {
                targetPlayerScript.Take_Damage_ServerRpc();
            }
        }

        await Awaitable.WaitForSecondsAsync(1);

        playerState.SetAttacking(false);
    }

    //--------------------------------------
    // Damage & Health Methods
    //--------------------------------------

    [ServerRpc(RequireOwnership = false)]
    private void Take_Damage_ServerRpc(int damage = 1)
    {
        Take_Damage(damage);
    }

    private async void Take_Damage(int damage = 1)
    {
        if (playerState.Stunning.Value && playerState.Dead.Value)
            return;

        playerState.SetStunning(true);

        playerState.TakeDamageServerRpc(damage);

        await Awaitable.WaitForSecondsAsync(2.0f);

        playerState.SetStunning(false);
    }


    //--------------------------------------
    // Network & Lifecycle Methods
    //--------------------------------------


    private void OnRoleChanged(PlayerRole previousValue, PlayerRole newValue)
    {
        if (IsOwner)
        {
            Debug.Log($"Your role changed to: {newValue}");
        }
    }


    //--------------------------------------
    // Spawn & Reset Methods
    //--------------------------------------

    public void ResetToSpawnPoint()
    {
        CharacterController.enabled = false;
        transform.position = SpawnPoint.GetClientSpawnPos(NetworkManager.Singleton.LocalClientId);
        CharacterController.enabled = true;
    }

    //--------------------------------------
    // Spectator
    //--------------------------------------

    private void OnSpectatorChanged(bool oldValue, bool newValue)
    {
        // Debug.Log($"Spectator changed to: {newValue}");
        if (newValue)
        {
            EnableSpectator();
        }
        else
        {
            DisableSpectator();
        }
    }

    private void EnableSpectator()
    {
        gameObject.layer = LayerMask.NameToLayer("Spectator");
        // CharacterController.excludeLayers = LayerMask.NameToLayer("Player");
        // Debug.Log(LayerMask.NameToLayer("Player"));

        if (IsLocalPlayer)
        {
            foreach (Player player in PlayerManager_Local.GetSpectatorPlayers(false))
            {
                player.gameObject.SetActive(true);
            }
        }
        else
        {
            if (!PlayerManager_Local.GetLocalPlayer().GetComponent<PlayerState>().Spectator.Value)
            {
                gameObject.SetActive(false);
            }
        }
    }


    private void DisableSpectator()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
        // CharacterController.excludeLayers = 0;

        gameObject.SetActive(true);
    }
}
