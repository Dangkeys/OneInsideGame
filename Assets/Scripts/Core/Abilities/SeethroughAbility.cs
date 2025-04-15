using System.Collections.Generic;
using UnityEngine;

public class SeethroughAbility : BaseAbility
{
    [field: SerializeField] public LayerMask DefaultLayerMask { get; private set; }
    [field: SerializeField] public LayerMask SeethroughLayerMask { get; private set; }

    private List<Player> otherPlayers = new List<Player>();
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
    protected override void OnAbilityActivated()
    {
        if (!IsOwner)
            return;
        otherPlayers = PlayerSystem.GetAllPlayer(player => !player.IsOwner);
        foreach (var player in otherPlayers)
        {
            StoreAndChangeLayer(player.gameObject, (int)Mathf.Log(SeethroughLayerMask.value, 2));
        }
    }

    protected override void OnAbilityCooldownStarted()
    {
        if (!IsOwner)
            return;

        foreach (var kvp in originalLayers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.layer = kvp.Value;
            }
        }

        originalLayers.Clear();
        otherPlayers.Clear();
    }

    protected override void OnAbilityReady()
    {

    }
    private void StoreAndChangeLayer(GameObject obj, int newLayer)
    {
        originalLayers[obj] = obj.layer;

        obj.layer = newLayer;


        foreach (Transform child in obj.transform)
        {
            StoreAndChangeLayer(child.gameObject, newLayer);
        }
    }


}