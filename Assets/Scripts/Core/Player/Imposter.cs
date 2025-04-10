using System.Threading.Tasks;
using OneInside.Constants;
using Unity.Netcode;
using UnityEngine;

public class Imposter : NetworkBehaviour
{
    //--------------------------------------
    // Public Variables
    //--------------------------------------
    public NetworkVariable<bool> CanTransformed = new NetworkVariable<bool>(true);
    public NetworkVariable<bool> Transformed = new NetworkVariable<bool>(false);

    public NetworkVariable<float> TransformationActiveTime = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.TRANSFORMATION_ACTIVE_TIME);
    public NetworkVariable<float> TransformationCooldownTime = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.TRANSFORMATION_COOLDOWN_TIME);
    public NetworkVariable<float> TransformationTime = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.TRANSFORMATION_TIME);

    //--------------------------------------
    // Private Variables
    //--------------------------------------
    private Player player;
    private PlayerState playerState;
    private PlayerMovement playerMovement;
    private BoxCollider[] hitBoxes;
    private CharacterController characterController;

    private Timer localTransformationCooldownTimer;
    private Timer localTransformationActiveTimer;

    private Timer serverTransformationCooldownTimer;
    private Timer serverTransformationActiveTimer;

    //--------------------------------------
    // Initialization & Cleanup
    //--------------------------------------

    public override void OnNetworkSpawn()
    {
        player = gameObject.GetComponent<Player>();
        playerState = player.GetComponent<PlayerState>();
        playerMovement = player.GetComponent<PlayerMovement>();

        hitBoxes = player.Hitbox.GetComponents<BoxCollider>();
        characterController = player.CharacterController;

        if (IsOwner)
        {
            player.InputReader.AttackEvent += Attack;
            player.InputReader.TranformationEvent += RequestToggleTransformation;
            Transformed.OnValueChanged += OnTransformedUpdate;
        }
    }

    public void Cleanup()
    {
        if (IsOwner)
        {
            player.InputReader.AttackEvent -= Attack;
            player.InputReader.TranformationEvent -= RequestToggleTransformation;
            Transformed.OnValueChanged -= OnTransformedUpdate;

        }
    }

    //--------------------------------------
    // Imposter Methods
    //--------------------------------------

    private async void Attack()
    {
        if (
            player.Role.Value != PlayerRole.Imposter
            || !Transformed.Value
            || !playerState.IsCanMove()
        )
            return;


        playerState.SetAttacking(true);

        var AllHitCharacters = HitboxUtilities.GetTouchingObjects(new HitboxUtilities.HitboxParams
        {
            Hitboxs = hitBoxes,
            Type = HitboxUtilities.ColliderType.CharacterController,
            Exclude = new Collider[] { characterController }
        });

        GameObject closestCharacter = GameUtilities.GetClosetTarget(transform.position, AllHitCharacters);

        if (closestCharacter != null)
        {
            Player targetPlayerScript = closestCharacter.GetComponent<Player>();
            if (targetPlayerScript && targetPlayerScript.IsAlive.Value && player.IsAlive.Value)
            {
                targetPlayerScript.TakeDamageServerRpc();
            }
        }

        await Awaitable.WaitForSecondsAsync(DefaultPlayerConfig.Imposter.ATTACK_COOLDOWN);

        playerState.SetAttacking(false);
    }

    //--------------------------------------
    // Transformation Methods
    //--------------------------------------

    private void RequestToggleTransformation()
    {
        ToggleTransformationServerRpc();
    }



    [ServerRpc]
    private void ToggleTransformationServerRpc(ServerRpcParams rpcParams = default)
    {
        if (player.Role.Value != PlayerRole.Imposter || !player.GetComponent<PlayerState>().IsCanMove())
            return;

        void EnableTransformed()
        {
            Transformed.Value = true;
            player.ServerSetCharacterID(player.ImposterCharacterID.Value);
        }

        void DisableTransformed()
        {
            Transformed.Value = false;
            player.ServerSetCharacterID(player.CrewmateCharacterID.Value);
        }

        if (!Transformed.Value)
        {
            if (!CanTransformed.Value)
                return;

            EnableTransformed();
            CanTransformed.Value = false;

            serverTransformationCooldownTimer?.Cancel();
            serverTransformationActiveTimer?.Cancel();

            serverTransformationCooldownTimer = Timer.Create(TransformationCooldownTime.Value, null, () =>
                    {
                        CanTransformed.Value = true;
                        serverTransformationCooldownTimer = null;
                    });
            serverTransformationActiveTimer = Timer.Create(TransformationActiveTime.Value, null, () =>
            {
                DisableTransformed();
                serverTransformationActiveTimer = null;
            });
        }
        else
        {
            DisableTransformed();

            if (serverTransformationActiveTimer != null)
            {
                serverTransformationActiveTimer.Cancel();
                serverTransformationActiveTimer = null;
            }
        }
    }

    //--------------------------------------
    // On Local Transformation Update
    //--------------------------------------

    private async void OnTransformedUpdate(bool oldValue, bool newValue)
    {
        if (!IsOwner || newValue == oldValue)
            return;

        if (newValue)
        {
            localTransformationCooldownTimer?.Cancel();
            localTransformationActiveTimer?.Cancel();

            localTransformationCooldownTimer = Timer.Create(TransformationCooldownTime.Value, (remainingTime) =>
            {
                Debug.Log("Transformation Cooldown TimeLeft: " + remainingTime);
            }, () =>
            {
                Debug.Log("Transformation Cooldown Finished!");
                localTransformationCooldownTimer = null;
            });

            localTransformationActiveTimer = Timer.Create(TransformationActiveTime.Value, (remainingTime) =>
            {
                Debug.Log("Transformation Active TimeLeft: " + remainingTime);
            }, () =>
            {
                Debug.Log("Transformation Active Finished!");
                localTransformationActiveTimer = null;
            });
        }
        else
        {
            if (localTransformationActiveTimer != null)
            {
                localTransformationActiveTimer.Cancel();
                localTransformationActiveTimer = null;
            }
        }

        playerMovement.EnablePlayerMovement(false);
        await Awaitable.WaitForSecondsAsync(TransformationTime.Value);
        playerMovement.EnablePlayerMovement(true);
    }
}
