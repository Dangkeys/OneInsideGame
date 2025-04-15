using System.Collections.Generic;
using UnityEngine;

public class DetectiveAbiliity : BaseAbility
{
    private List<Player> otherPlayers = new List<Player>();
    protected override void OnAbilityActivated()
    {
        if (!IsOwner)
            return;

        otherPlayers = PlayerSystem.GetAllPlayer(player => !player.IsOwner);

        SetPlayerTrailsVisibility(true);
    }

    protected override void OnAbilityCooldownStarted()
    {
        if (!IsOwner)
            return;

        SetPlayerTrailsVisibility(false);
    }

    protected override void OnAbilityReady()
    {

    }
    private void SetPlayerTrailsVisibility(bool shouldShow)
    {
        foreach (var player in otherPlayers)
        {
            var particleSystem = player.gameObject.GetComponentInChildren<ParticleSystem>();
            if (particleSystem != null)
            {
                var emission = particleSystem.emission;
                emission.enabled = shouldShow;
            }
        }
    }
}
