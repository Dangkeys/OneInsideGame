using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class DisguiseAbility : BaseAbility
{
    [field: SerializeField] public LayerMask DefaultLayerMask { get; private set; }
    [field: SerializeField] public LayerMask FadedLayerMask { get; private set; }


    public override void Activate()
    {
        if (!IsOwner)
            return;
        ShouldDisguiseServerRpc(OwnerClientId, true);
    }

    public override void DeActivate()
    {
        if (!IsOwner)
            return;
        ShouldDisguiseServerRpc(OwnerClientId, false);
    }
    [ServerRpc]
    private void ShouldDisguiseServerRpc(ulong disguiseClientId, bool shouldDisguise)
    {
        if (!IsHost)
        {
            ShouldDisguise(disguiseClientId, shouldDisguise);
        }
        ShouldDisguiseClientRpc(disguiseClientId, shouldDisguise);

    }
    [ClientRpc]
    private void ShouldDisguiseClientRpc(ulong disguiseClientId, bool shouldDisguise)
    {
        ShouldDisguise(disguiseClientId, shouldDisguise);
    }


    private void ShouldDisguise(ulong disguiseClientId, bool shouldDisguise)
    {
        var disguisePlayer = PlayerSystem.GetPlayerScriptByClientId(disguiseClientId);
        if (IsOwner)
        {
            if (shouldDisguise)
            {
                Debug.Log(disguisePlayer.AbilityData);
                StoreAndChangeLayer(disguisePlayer.gameObject, (int)Mathf.Log(FadedLayerMask.value, 2));
            }
            else
            {
                StoreAndChangeLayer(disguisePlayer.gameObject, (int)Mathf.Log(DefaultLayerMask.value, 2));
            }
        }
        else
        {
            disguisePlayer.transform.Find("PlayerVisual").gameObject.SetActive(!shouldDisguise);
        }
    }
    private void StoreAndChangeLayer(GameObject obj, int newLayer)
    {

        obj.layer = newLayer;


        foreach (Transform child in obj.transform)
        {
            StoreAndChangeLayer(child.gameObject, newLayer);
        }
    }

    public override bool CanActivate() => true;
}