using System;
using OneInside.Constants;
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

    [Header("Spectator")]
    [field: SerializeField] public Material SpectatorMaterial { get; private set; }
    [field: SerializeField] public GameObject PlayerVisual { get; private set; }

    [Header("Status")]
    public NetworkVariable<bool> IsAlive = new NetworkVariable<bool>(true);
    public NetworkVariable<PlayerRole> Role = new NetworkVariable<PlayerRole>(PlayerRole.None);

    [Header("Characters")]
    public NetworkVariable<FixedString64Bytes> CrewmateCharacterID = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<FixedString64Bytes> ImposterCharacterID = new NetworkVariable<FixedString64Bytes>();

    public NetworkVariable<FixedString64Bytes> CurrentCharacterID = new NetworkVariable<FixedString64Bytes>();

    //--------------------------------------
    // Private Variables
    //--------------------------------------

    private PlayerState playerState;
    private Renderer playerRenderer;
    private Material defaultPlayerMaterial;
    private PlayerSystem playerManager;
    private Imposter imposter;
    public AbilityDataSO AbilityData { get; private set; }
    public event Action OnAbilityDataChanged;
    //--------------------------------------
    // Unity Lifecycle Methods
    //--------------------------------------

    void Awake()
    {
        playerManager = OneInsideLevelSystem.Instance.PlayerSystem;
        

        CrewmateCharacterID.Value = CharacterManager.Instance.DefaultCrewmateCharacter.ID;
        ImposterCharacterID.Value = CharacterManager.Instance.DefaultImposterCharacter.ID;

        CurrentCharacterID.Value = CrewmateCharacterID.Value;
    }

    public override void OnNetworkSpawn()
    {
        playerState = GetComponent<PlayerState>();
        imposter = GetComponent<Imposter>();

        playerRenderer = CharacterManager.GetCharacterSkin(PlayerVisual).GetComponent<Renderer>();
        defaultPlayerMaterial = playerRenderer.material;

        Role.OnValueChanged += OnRoleChanged;
        IsAlive.OnValueChanged += OnAliveChanged;



        if (!IsOwner)
        {
            // Other Player
            VirtualCamera.Priority = int.MinValue;
        }
        else
        {
            // Owner Player

            CurrentCharacterID.OnValueChanged += OnCharacterIDChanged;

            playerState.SetAttacking(false);
            playerState.SetStunning(false);
            playerState.SetWalking(false);
            playerState.SetRunning(false);
            playerState.SetAlive(true);

            if (playerManager != null)
            {
                playerManager.OnResetALlPlayerPosition += ResetToSpawnPoint;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        Role.OnValueChanged -= OnRoleChanged;
        IsAlive.OnValueChanged -= OnAliveChanged;

        if (!IsOwner)
            return;

        CurrentCharacterID.OnValueChanged -= OnCharacterIDChanged;

        imposter?.Cleanup();

        if (playerManager != null)
        {
            playerManager.OnResetALlPlayerPosition -= ResetToSpawnPoint;
        }
    }

    //--------------------------------------
    // Character Management
    //--------------------------------------

    public void OnCharacterIDChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        CharacterManager.Instance.ChangeCharacterServerRpc(newValue.ToString());
    }

    [ServerRpc]
    public void SetCharacterIDServerRpc(FixedString64Bytes character)
    {
        CurrentCharacterID.Value = character;
    }

    public void ServerSetCharacterID(FixedString64Bytes newValue)
    {
        CharacterManager.Instance.ChangeCharacterClientRpc(OwnerClientId, newValue.ToString(), Character.SearchType.Default);
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
            playerState.SetAliveServerRpc(true);
            playerState.SetHealthServerRpc(playerState.MaxHealth.Value);
        }

        // if (Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     SetCharacterIDServerRpc("Psycho");
        // }
        // if (Input.GetKeyDown(KeyCode.Alpha2))
        // {
        //     SetCharacterIDServerRpc("Warewolf");
        // }
    }

    //--------------------------------------
    // Damage & Health Methods
    //--------------------------------------

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage = DefaultPlayerConfig.Imposter.DAMAGE, ServerRpcParams serverRpcParams = default)
    {
        // Check if the sender is an Imposter and the target (this player) is alive
        if (PlayerSystem.GetPlayerRoleByClientId(serverRpcParams.Receive.SenderClientId) == PlayerRole.Imposter && IsAlive.Value)
        {
            TakeDamage(damage);
        }
    }

    private async void TakeDamage(float damage)
    {
        if (playerState.Stunning.Value || !IsAlive.Value)
            return;

        playerState.SetStunning(true);
        playerState.TakeDamageServerRpc(damage);

        await Awaitable.WaitForSecondsAsync(DefaultPlayerConfig.Crewmate.STUN_DURATION);

        playerState.SetStunning(false);
    }


    //--------------------------------------
    // Role Change Handling
    //--------------------------------------

    private void OnRoleChanged(PlayerRole previousValue, PlayerRole newValue)
    {

        if (Role.Value == PlayerRole.Imposter)
        {
            imposter.enabled = true;
        }
        else
        {
            imposter.enabled = false;
        }

        if (IsOwner)
        {
            UIManager.Instance.ShowMessage($"YOUR ROLE IS {newValue.ToString().ToUpper()}");
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
    // Spectator Mode Handling
    //--------------------------------------

    private void OnAliveChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            DisableSpectator();
        }
        else
        {
            EnableSpectator();
        }
    }

    private void EnableSpectator()
    {
        if (playerRenderer == null || SpectatorMaterial == null)
            return;

        gameObject.layer = LayerMask.NameToLayer(OneInsideLayers.SPECTATOR);
        playerRenderer.material = SpectatorMaterial;

        if (IsOwner)
        {
            foreach (Player player in PlayerSystem.GetSpectatorPlayers(false))
            {
                if (player != this)
                    player.gameObject.SetActive(true);
            }
        }
        else
        {
            gameObject.SetActive(!PlayerSystem.GetLocalPlayerScript().IsAlive.Value);
        }
    }


    private void DisableSpectator()
    {
        if (playerRenderer == null || defaultPlayerMaterial == null)
            return;

        gameObject.layer = LayerMask.NameToLayer(OneInsideLayers.PLAYER);
        playerRenderer.material = defaultPlayerMaterial;

        gameObject.SetActive(true);

        if (IsOwner)
        {
            foreach (Player player in PlayerSystem.GetSpectatorPlayers(false))
            {
                if (player != this)
                    player.gameObject.SetActive(false);
            }
        }
    }
    [ClientRpc]
    public void SetAbilityDataSOClientRpc(string abilityDataId, ClientRpcParams clientRpcParams)
    {
        AbilityData = OneInsideLevelSystem.Instance.AbilityAssignment.AbilityCollection.GetAbilityById(abilityDataId);
        OnAbilityDataChanged?.Invoke();
    }
}
