using System.Collections.Generic;
using UnityEngine;

public class DetectiveAbility : BaseAbility
{
    private List<Player> otherPlayers = new List<Player>();
    public override void Activate()
    {
        if (!IsOwner)
            return;

        otherPlayers = PlayerManager.GetAllPlayer(player => !player.IsOwner);

        SetPlayerTrailsVisibility(true);
    }
    public override bool CanActivate() => true;
    public override void DeActivate()
    {
        if (!IsOwner)
            return;

        SetPlayerTrailsVisibility(false);
    }

    private void SetPlayerTrailsVisibility(bool shouldShow)
    {
        foreach (var player in otherPlayers)
        {
            var trail = player.gameObject.GetComponentInChildren<TrailRenderer>();
            if (trail != null)
            {
                trail.emitting = shouldShow;
            }
        }
    }
}