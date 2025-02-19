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

    public NetworkVariable<bool> IsAlive = new NetworkVariable<bool>(true);

    public NetworkVariable<PlayerRole> Role = new NetworkVariable<PlayerRole>(PlayerRole.None);

    //--------------------------------------
    // Private Variables
    //--------------------------------------

    private BoxCollider[] hitBoxes;
    private PlayerState playerState;
    private Renderer playerRenderer;
    private Material defaultPlayerMaterial;

    private PlayerManager playerManager;

    void Awake()
    {
        playerManager = OneInsideLevelManager.Instance.PlayerManager;
    }

    public override void OnNetworkSpawn()
    {
        hitBoxes = Hitbox.GetComponents<BoxCollider>();
        playerState = GetComponent<PlayerState>();

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
            InputReader.AttackEvent += OnAttackLocal;

            playerState.SetAttacking(false);
            playerState.SetStunning(false);
            playerState.SetWalking(false);
            playerState.SetRunning(false);
            playerState.SetAlive(true);

            if (!playerManager)
                return;
            playerManager.OnResetALlPlayerPosition += ResetToSpawnPoint;
        }
    }

    public override void OnNetworkDespawn()
    {
        Role.OnValueChanged -= OnRoleChanged;
        IsAlive.OnValueChanged -= OnAliveChanged;

        if (!IsOwner)
            return;

        InputReader.AttackEvent -= OnAttackLocal;

        if (!playerManager)
            return;
        playerManager.OnResetALlPlayerPosition -= ResetToSpawnPoint;

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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OneInsideGameManager.Instance.CharacterManager.ChangeCharacterServerRpc(OneInsideGameManager.Instance.CharacterManager.CharactersCollection.Characters[0].name);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OneInsideGameManager.Instance.CharacterManager.ChangeCharacterServerRpc(OneInsideGameManager.Instance.CharacterManager.CharactersCollection.Characters[1].name);
        }
    }
    //--------------
    // Attack Methods
    //--------------------------------------

    public async void OnAttackLocal()
    {
        if (!playerState.IsCanMove())
            return;

        playerState.SetAttacking(true);

        // get closest character
        var AllHitCharacters = AzHitbox.GetTouchingObjects(new AzHitbox.HitboxParams
        {
            Hitboxs = hitBoxes,
            Type = AzHitbox.ColliderType.CharacterController,
            Exclude = new Collider[] { CharacterController }
        });

        GameObject closestCharacter = AzNormal.GetClosetTarget(transform.position, AllHitCharacters);

        if (closestCharacter != null)
        {
            Player targetPlayerScript = closestCharacter.GetComponent<Player>();
            if (targetPlayerScript && targetPlayerScript.IsAlive.Value && PlayerManager.GetLocalPlayerScript().IsAlive.Value)
            {
                targetPlayerScript.TakeDamageServerRpc();
            }
        }

        await Awaitable.WaitForSecondsAsync(1);

        playerState.SetAttacking(false);
    }

    //--------------------------------------
    // Damage & Health Methods
    //--------------------------------------

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage = 1)
    {
        TakeDamage(damage);
    }

    private async void TakeDamage(int damage = 1)
    {
        if (playerState.Stunning.Value || !IsAlive.Value)
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
            OneInsideGameManager.Instance.ShowMessage($"YOUR ROLE IS {newValue.ToString().ToUpper()}");
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
        gameObject.layer = LayerMask.NameToLayer(OneInsideLayers.SPECTATOR);
        playerRenderer.material = SpectatorMaterial;

        if (IsOwner)
        {
            foreach (Player player in PlayerManager.GetSpectatorPlayers(false))
            {
                player.gameObject.SetActive(true);
            }
        }
        else
        {
            gameObject.SetActive(!PlayerManager.GetLocalPlayerScript().IsAlive.Value);
        }
    }


    private void DisableSpectator()
    {
        gameObject.layer = LayerMask.NameToLayer(OneInsideLayers.PLAYER);
        playerRenderer.material = defaultPlayerMaterial;

        gameObject.SetActive(true);

        if (IsOwner)
        {
            foreach (Player player in PlayerManager.GetSpectatorPlayers(false))
            {
                player.gameObject.SetActive(false);
            }
        }
    }
}
