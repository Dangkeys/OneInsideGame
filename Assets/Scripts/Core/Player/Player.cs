using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OneInside.Constants;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{

    [Header("References")]
    [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
    public InputReader InputReader { get; private set; }
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
    public Inventory Inventory;
    [SerializeField] ItemCollectionSO itemCollection;
    [SerializeField] GameObject dropHitbox;
    public AbilityDataSO AbilityData { get; private set; }
    public event Action OnAbilityDataChanged;

    private BoxCollider[] hitBoxes;

    private float pokeStunDuration = DefaultPlayerConfig.Player.POKE_STUN_DURATION;
    private float pokeCoolDown = DefaultPlayerConfig.Player.POKE_COOLDOWN;

    //--------------------------------------
    // Unity Lifecycle Methods
    //--------------------------------------

    void Awake()
    {
        playerManager = OneInsideLevelSystem.Instance.PlayerSystem;
        InputReader = InputReader.Instance;

        Inventory = new Inventory();
    }

    public override void OnNetworkSpawn()
    {
        playerState = GetComponent<PlayerState>();
        imposter = GetComponent<Imposter>();

        hitBoxes = Hitbox.GetComponents<BoxCollider>();

        playerRenderer = CharacterManager.GetCharacterSkin(PlayerVisual).GetComponent<Renderer>();
        defaultPlayerMaterial = playerRenderer.material;

        if (IsServer)
        {
            CrewmateCharacterID.Value = CharacterManager.Instance.DefaultCrewmateCharacter.ID;
            ImposterCharacterID.Value = CharacterManager.Instance.DefaultImposterCharacter.ID;
        }


        CurrentCharacterID.Value = CrewmateCharacterID.Value;

        Role.OnValueChanged += OnRoleChanged;
        IsAlive.OnValueChanged += OnAliveChanged;

        InventoryEdit.OnCraftItemA += CraftItemA;
        InventoryEdit.OnDropItem += DropItem;



        if (!IsOwner)
        {
            VirtualCamera.Priority = int.MinValue;
        }
        else
        {
            InputReader.AttackEvent += OnAttack;

            CurrentCharacterID.OnValueChanged += OnCharacterIDChanged;

            playerState.SetAttacking(false);
            playerState.SetPoking(false);
            playerState.SetWalking(false);
            playerState.SetRunning(false);

            if (playerManager != null)
            {
                playerManager.OnResetALlPlayerPosition += ResetToSpawnPoint;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        InputReader.AttackEvent -= OnAttack;

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
    }

    //--------------------------------------
    // Damage & Health Methods
    //--------------------------------------

    public Player GetPlayerInHitbox()
    {
        var AllHitCharacters = HitboxUtilities.GetTouchingObjects(new HitboxUtilities.HitboxParams
        {
            Hitboxs = hitBoxes,
            Type = HitboxUtilities.ColliderType.CharacterController,
            Exclude = new Collider[] { CharacterController }
        });

        GameObject closestCharacter = GameUtilities.GetClosetTarget(transform.position, AllHitCharacters);
        Player targetPlayerScript = null;

        if (closestCharacter != null)
        {
            targetPlayerScript = closestCharacter.GetComponent<Player>();
        }

        return targetPlayerScript;
    }

    private void OnAttack()
    {
        if (!playerState.IsCanMove() || !IsAlive.Value || PlayerMovement.Behaviour == MovementBehaviour.STUNNING)
            return;

        Player targetPlayerScript = GetPlayerInHitbox();
        switch (Role.Value)
        {
            case PlayerRole.Imposter:
                if (imposter.Transformed.Value)
                {
                    imposter.Attack(targetPlayerScript);
                }
                else
                {
                    Poke(targetPlayerScript);
                }
                break;
            default:
                Poke(targetPlayerScript);
                break;
        }
    }

    private async void Poke(Player targetPlayerScript)
    {
        if (playerState.Poking.Value)
            return;

        if (targetPlayerScript)
        {
            targetPlayerScript.TakeDamageServerRpc(0);
        }

        playerState.SetPoking(true);

        PlayerMovement.SetMovementBehavior(MovementBehaviour.STUNNING);
        await Awaitable.WaitForSecondsAsync(pokeStunDuration);
        PlayerMovement.SetMovementBehavior(MovementBehaviour.DEFAULT);

        await Awaitable.WaitForSecondsAsync(pokeCoolDown - pokeStunDuration);
        playerState.SetPoking(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage = DefaultPlayerConfig.Imposter.DAMAGE, ServerRpcParams serverRpcParams = default)
    {
        Player senderPlayer = PlayerSystem.GetPlayerByClientId(serverRpcParams.Receive.SenderClientId);

        if (!senderPlayer.IsAlive.Value)
            return;

        if (senderPlayer.Role.Value != PlayerRole.Imposter)
        {
            damage = 0;
        }

        if (IsAlive.Value)
        {
            if (damage > 0)
            {
                playerState.ChangeHealthServerRpc(damage);

            }
            Stunning();
        }
    }

    private async void Stunning()
    {
        if (!IsAlive.Value)
            return;

        playerState.SetStunning(true);
        PlayerMovement.SetMovementBehavior(MovementBehaviour.STUNNING);

        await Awaitable.WaitForSecondsAsync(DefaultPlayerConfig.Player.POKED_STUN_DURATION);

        playerState.SetStunning(false);
        PlayerMovement.SetMovementBehavior(MovementBehaviour.DEFAULT);
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

    //--------------------------------------
    // Inventory Editting Handling
    //--------------------------------------

    private void CraftItemA()
    {
        Inventory.CraftItemA();
    }

    private void DropItem()
    {
        NetworkObject itemToDrop = null;
        Item.ItemType droppedItem = Inventory.DropItem();
        if (droppedItem != Item.ItemType.None)
        {
            switch (droppedItem)
            {
                case Item.ItemType.IngredientA:
                    itemToDrop = itemCollection.Ingredients[0];
                    break;
                case Item.ItemType.IngredientB:
                    itemToDrop = itemCollection.Ingredients[1];
                    break;
                case Item.ItemType.IngredientC:
                    itemToDrop = itemCollection.Ingredients[2];
                    break;
                case Item.ItemType.IngredientD:
                    itemToDrop = itemCollection.Ingredients[3];
                    break;
                case Item.ItemType.IngredientE:
                    itemToDrop = itemCollection.Ingredients[4];
                    break;
                case Item.ItemType.ItemA:
                    itemToDrop = itemCollection.Items[0];
                    break;
                case Item.ItemType.ItemB:
                    itemToDrop = itemCollection.Items[1];
                    break;
                case Item.ItemType.ItemC:
                    itemToDrop = itemCollection.Items[2];
                    break;
            }

            NetworkObject itemObject = Instantiate(itemToDrop, dropHitbox.transform.position, Quaternion.identity);
            if (itemObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                networkObject.Spawn();
            }
            else
            {
                Debug.LogError("Dropped item does not have an Item component.");
            }
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
