using UnityEngine;

public class SprintAbility : BaseAbility
{
    private float normalWalkSpeed;
    private float normalRunSpeed;

    [SerializeField] private float speedMultiplier = 1.25f;
    public override void Activate()
    {
        if (!IsOwner)
            return;

        var localPlayer = PlayerSystem.GetLocalPlayer();

        if (localPlayer.TryGetComponent(out PlayerMovement playerMovement))
        {
            normalWalkSpeed = playerMovement.WalkSpeed;
            normalRunSpeed = playerMovement.RunSpeed;
            playerMovement.SetWalkSpeed(normalWalkSpeed * speedMultiplier);
            playerMovement.SetRunSpeed(normalRunSpeed * speedMultiplier);
        }

    }

    public override bool CanActivate() => true;

    public override void DeActivate()
    {
        if (!IsOwner)
            return;

        var localPlayer = PlayerSystem.GetLocalPlayer();

        if (localPlayer.TryGetComponent(out PlayerMovement playerMovement))
        {
            playerMovement.SetWalkSpeed(normalWalkSpeed);
            playerMovement.SetRunSpeed(normalRunSpeed);
        }
    }
}
