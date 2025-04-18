using UnityEngine;

public class SprintAbility : BaseAbility
{
    [Header("Sprint Settings")]
    [SerializeField] private float speedMultiplier = 1.25f;
    [Tooltip("Multiplies player's walk and run speeds when ability is active")]

    private float normalWalkSpeed;
    private float normalRunSpeed;

    protected override void OnAbilityActivated()
    {
        if (!IsOwner)
            return;

        var localPlayer = PlayerSystem.GetLocalPlayer();
        if (localPlayer == null)
            return;

        if (localPlayer.TryGetComponent(out PlayerMovement playerMovement))
        {

            normalWalkSpeed = playerMovement.WalkSpeed;
            normalRunSpeed = playerMovement.RunSpeed;
            
            playerMovement.SetWalkSpeed(normalWalkSpeed * speedMultiplier);
            playerMovement.SetRunSpeed(normalRunSpeed * speedMultiplier);
        }
    }

    protected override void OnAbilityCooldownStarted()
    {
        if (!IsOwner) 
            return;
        
        var localPlayer = PlayerSystem.GetLocalPlayer();
        if (localPlayer == null)
            return;

        if (localPlayer.TryGetComponent(out PlayerMovement playerMovement))
        {
            playerMovement.SetWalkSpeed(normalWalkSpeed);
            playerMovement.SetRunSpeed(normalRunSpeed);
        }
    }
}