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

    public NetworkVariable<float> BasedAttackStunDuration = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.ATTACK_STUN_DURATION,
           NetworkVariableReadPermission.Everyone,
           NetworkVariableWritePermission.Server
    );
    public NetworkVariable<float> AttackStunDuration = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.ATTACK_STUN_DURATION,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> BasedAttackCooldown = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.ATTACK_COOLDOWN,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<float> AttackCooldown = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.ATTACK_COOLDOWN,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> EndGameCoolDownFactor = new NetworkVariable<float>(DefaultPlayerConfig.Imposter.END_GAME_COOLDOWN_FACTOR);

    public Timer ServerTransformationCooldownTimer;
    public Timer ServerTransformationActiveTimer;

    //--------------------------------------
    // Private Variables
    //--------------------------------------
    private Player player;
    private PlayerState playerState;
    private PlayerMovement playerMovement;

    private Timer localTransformationCooldownTimer;
    private Timer localTransformationActiveTimer;


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
            ForceTransform.OnValueChanged += OnServerForceTransformUpdate;
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
            ForceTransform.OnValueChanged -= OnServerForceTransformUpdate;
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

        await Awaitable.WaitForSecondsAsync(AttackStunDuration.Value);
        playerMovement.SetMovementBehavior(MovementBehaviour.DEFAULT);

        await Awaitable.WaitForSecondsAsync(AttackCooldown.Value - AttackStunDuration.Value);
        playerState.SetAttacking(false);
    }

    //--------------------------------------
    // Transformation Methods
    //--------------------------------------

    void EnableTransformed(Player player)
    {
        Imposter imposter = player.GetComponent<Imposter>();

        imposter.ServerTransformationCooldownTimer?.Cancel();
        imposter.ServerTransformationActiveTimer?.Cancel();
        Transformed.Value = true;
        Debug.Log("Transformed");
        Debug.Log(player.CurrentCharacterID.Value != player.ImposterCharacterID.Value);
        if (player.CurrentCharacterID.Value != player.ImposterCharacterID.Value)
            player.ServerSetCharacterID(player, player.ImposterCharacterID.Value);
    }

    void DisableTransformed(Player player)
    {
        Imposter imposter = player.GetComponent<Imposter>();

        imposter.ServerTransformationActiveTimer?.Cancel();
        imposter.ServerTransformationActiveTimer = null;
        if (!Transformed.Value)
            return;
        Transformed.Value = false;
        if (player.CurrentCharacterID.Value != player.CrewmateCharacterID.Value)
            player.ServerSetCharacterID(player, player.CrewmateCharacterID.Value);
    }

    private void OnServerForceTransformUpdate(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            EnableTransformed(player);
            AttackStunDuration.Value = BasedAttackStunDuration.Value * EndGameCoolDownFactor.Value;
            AttackCooldown.Value = BasedAttackCooldown.Value * EndGameCoolDownFactor.Value;
        }
        else
        {
            DisableTransformed(player);
            AttackStunDuration.Value = BasedAttackStunDuration.Value;
            AttackCooldown.Value = BasedAttackCooldown.Value;
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
        Debug.Log(rpcParams.Receive.SenderClientId);
        Player thisPlayer = PlayerSystem.GetPlayerByClientId(rpcParams.Receive.SenderClientId);
        Imposter thisImposter = thisPlayer.GetComponent<Imposter>();

        if (thisPlayer.Role.Value != PlayerRole.Imposter || !thisPlayer.GetComponent<PlayerState>().IsCanMove() || thisImposter.ForceTransform.Value)
            return;

        if (!Transformed.Value)
        {
            if (!CanTransformed.Value)
                return;

            EnableTransformed(thisPlayer);
            CanTransformed.Value = false;

            thisImposter.ServerTransformationCooldownTimer = Timer.Create(TransformationCooldownTime.Value, null, () =>
            {
                CanTransformed.Value = true;
                ServerTransformationCooldownTimer = null;
            });

            thisImposter.ServerTransformationActiveTimer = Timer.Create(TransformationActiveTime.Value, null, () =>
            {
                DisableTransformed(thisPlayer);
                thisImposter.ServerTransformationActiveTimer = null;
            });
        }
        else
        {
            DisableTransformed(thisPlayer);
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
                // Debug.Log("Transformation Cooldown TimeLeft: " + remainingTime);
            }, () =>
            {
                Debug.Log("Transformation Cooldown Finished!");
                localTransformationCooldownTimer = null;
            });

            localTransformationActiveTimer = Timer.Create(TransformationActiveTime.Value, (remainingTime) =>
            {
                // Debug.Log("Transformation Active TimeLeft: " + remainingTime);
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
