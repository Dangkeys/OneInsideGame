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
    public NetworkVariable<bool> ForceTransform = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    public NetworkVariable<float> TransformationActiveTime = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.TRANSFORMATION_ACTIVE_TIME);
    public NetworkVariable<float> TransformationCooldownTime = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.TRANSFORMATION_COOLDOWN_TIME);
    public NetworkVariable<float> TransformationTime = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.TRANSFORMATION_TIME);

    //--------------------------------------
    // Private Variables
    //--------------------------------------
    private Player player;
    private PlayerState playerState;
    private PlayerMovement playerMovement;

    private Timer localTransformationCooldownTimer;
    private Timer localTransformationActiveTimer;

    private Timer serverTransformationCooldownTimer;
    private Timer serverTransformationActiveTimer;

    private float attackStunDuration = DefaultPlayerConfig.Imposter.ATTACK_STUN_DURATION;
    private float attackCoolDown = DefaultPlayerConfig.Imposter.ATTACK_COOLDOWN;

    //--------------------------------------
    // Initialization & Cleanup
    //--------------------------------------

    public override void OnNetworkSpawn()
    {
        player = gameObject.GetComponent<Player>();
        playerState = player.GetComponent<PlayerState>();
        playerMovement = player.GetComponent<PlayerMovement>();



        if (IsOwner)
        {
            player.InputReader.TranformationEvent += RequestToggleTransformation;
            Transformed.OnValueChanged += OnTransformedUpdate;
            ForceTransform.OnValueChanged += OnForceTransformUpdateLocal;
        }

        if (IsServer)
        {
            ForceTransform.OnValueChanged += OnForceTransformUpdate;
        }
    }

    public void Cleanup()
    {
        if (IsOwner)
        {
            player.InputReader.TranformationEvent -= RequestToggleTransformation;
            Transformed.OnValueChanged -= OnTransformedUpdate;
            ForceTransform.OnValueChanged -= OnForceTransformUpdateLocal;
        }

        if (IsServer)
        {
            ForceTransform.OnValueChanged -= OnForceTransformUpdate;
        }
    }

    //--------------------------------------
    // Imposter Methods
    //--------------------------------------

    public async void Attack(Player targetPlayerScript)
    {
        if (
            player.Role.Value != PlayerRole.Imposter
            || !Transformed.Value
            || !playerState.IsCanMove() || playerState.Attacking.Value
        )
            return;


        if (targetPlayerScript && targetPlayerScript.IsAlive.Value && player.IsAlive.Value)
        {
            targetPlayerScript.TakeDamageServerRpc();
        }

        playerState.SetAttacking(true);
        playerMovement.SetMovementBehavior(MovementBehaviour.STUNNING);

        await Awaitable.WaitForSecondsAsync(attackStunDuration);
        playerMovement.SetMovementBehavior(MovementBehaviour.DEFAULT);

        await Awaitable.WaitForSecondsAsync(attackCoolDown - attackStunDuration);
        playerState.SetAttacking(false);
    }

    //--------------------------------------
    // Transformation Methods
    //--------------------------------------

    void EnableTransformed()
    {
        serverTransformationCooldownTimer?.Cancel();
        serverTransformationActiveTimer?.Cancel();
        Transformed.Value = true;
        if (player.CurrentCharacterID.Value != player.ImposterCharacterID.Value)
            player.SetCharacterIDServerRpc(player.ImposterCharacterID.Value);
    }

    void DisableTransformed()
    {
        serverTransformationActiveTimer?.Cancel();
        serverTransformationActiveTimer = null;
        if (!Transformed.Value)
            return;
        Transformed.Value = false;
        if (player.CurrentCharacterID.Value != player.CrewmateCharacterID.Value)
            player.SetCharacterIDServerRpc(player.CrewmateCharacterID.Value);
    }

    private void OnForceTransformUpdate(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            EnableTransformed();
        }
        else
        {
            DisableTransformed();
        }
    }

    private void OnForceTransformUpdateLocal(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            localTransformationCooldownTimer?.Cancel();
            localTransformationActiveTimer?.Cancel();
        }
    }

    private void RequestToggleTransformation()
    {
        ToggleTransformationServerRpc();
    }

    [ServerRpc]
    private void ToggleTransformationServerRpc(ServerRpcParams rpcParams = default)
    {
        if (player.Role.Value != PlayerRole.Imposter || !player.GetComponent<PlayerState>().IsCanMove() || ForceTransform.Value)
            return;

        if (!Transformed.Value)
        {
            if (!CanTransformed.Value)
                return;

            EnableTransformed();
            CanTransformed.Value = false;

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
        }
    }

    //--------------------------------------
    // On Local Transformation Update
    //--------------------------------------

    private async void OnTransformedUpdate(bool oldValue, bool newValue)
    {
        if (!IsOwner || newValue == oldValue)
            return;

        playerMovement.EnablePlayerMovement(false);
        await Awaitable.WaitForSecondsAsync(TransformationTime.Value);
        playerMovement.EnablePlayerMovement(true);

        if (newValue)
        {
            localTransformationCooldownTimer?.Cancel();
            localTransformationActiveTimer?.Cancel();

            if (ForceTransform.Value)
                return;


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
            localTransformationActiveTimer?.Cancel();
            localTransformationActiveTimer = null;

        }


    }
}
