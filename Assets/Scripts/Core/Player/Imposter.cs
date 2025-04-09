using OneInside.Constants;
using Unity.Netcode;
using UnityEngine;

public class Imposter : MonoBehaviour
{
    //--------------------------------------
    // Private Variables
    //--------------------------------------
    private Player player;
    private PlayerState playerState;
    private BoxCollider[] hitBoxes;
    private CharacterController characterController;

    //--------------------------------------
    // Initialization & Cleanup
    //--------------------------------------

    public void Initialize(Player ownerPlayer)
    {
        player = ownerPlayer;
        playerState = player.GetComponent<PlayerState>();
        hitBoxes = player.Hitbox.GetComponents<BoxCollider>();
        characterController = player.CharacterController;

        if (player.IsOwner)
        {
            player.InputReader.AttackEvent += OnAttackLocal;
        }
    }

    public void Cleanup()
    {
        if (player != null && player.IsOwner && player.InputReader != null)
        {
            player.InputReader.AttackEvent -= OnAttackLocal;
        }
    }

    //--------------------------------------
    // Imposter Attack Methods
    //--------------------------------------

    private async void OnAttackLocal()
    {
        if (player.Role.Value != PlayerRole.Imposter)
            return;

        if (!playerState.IsCanMove())
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
}
