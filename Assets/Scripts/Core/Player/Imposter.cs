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



        if (IsOwner)
        {
            player.InputReader.TranformationEvent += RequestToggleTransformation;
            Transformed.OnValueChanged += OnTransformedUpdate;
        }
    }

    public void Cleanup()
    {
        if (IsOwner)
        {
            player.InputReader.TranformationEvent -= RequestToggleTransformation;
            Transformed.OnValueChanged -= OnTransformedUpdate;

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
