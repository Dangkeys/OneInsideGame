using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class AbilityAssignment : NetworkBehaviour
{
    [field: SerializeField] public AbilityCollectionSO AbilityCollection { get; private set; }
    private OneInsideLevelManager oneInsideLevelManager;
    private RoleManager roleManager;
    private NetworkPlayerData networkPlayerData;
    private OneInsideGameManager oneInsideGameManager;
    public override void OnNetworkSpawn()
    {



        oneInsideLevelManager = OneInsideLevelManager.Instance;
        roleManager = oneInsideLevelManager.RoleManager;
        oneInsideGameManager = OneInsideGameManager.Instance;
        if (!oneInsideGameManager)
            return;
        networkPlayerData = oneInsideGameManager.NetcodeManager.NetworkPlayerData;
        if (IsServer)
        {
            roleManager.OnRolesAssignmentComplete += AssignAbilities;
        }
    }



    private void AssignAbilities()
    {
        var players = PlayerManager.GetAllPlayer(null);
        foreach (var player in players)
        {
            var abilityData = GetPlayerAbilityData(player);
            if (abilityData != null)
            {
                var clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { player.OwnerClientId }
                    }
                };
                SpawnAbility(player, abilityData);
                player.SetAbilityDataSOClientRpc(abilityData.Id, clientRpcParams);
            }
        }
    }
    private AbilityDataSO GetPlayerAbilityData(Player player)
    {
        var role = player.Role.Value;

        if (networkPlayerData != null)
        {
            var userData = networkPlayerData.GetUserDataFromClientId(player.OwnerClientId);
            if (userData != null)
            {
                bool hasValidAbilityId = role == PlayerRole.CREWMATE
                    ? !string.IsNullOrEmpty(userData.CrewAbilityId)
                    : !string.IsNullOrEmpty(userData.ImposterAbilityId);

                if (hasValidAbilityId)
                {
                    return GetNetworkedAbility(player, role);
                }
                else
                {
                    return GetRandomAbility(role);
                }
            }
        }

        return GetRandomAbility(role);
    }

    private AbilityDataSO GetNetworkedAbility(Player player, PlayerRole role)
    {
        var userData = networkPlayerData.GetUserDataFromClientId(player.OwnerClientId);
        var abilityId = role == PlayerRole.CREWMATE
            ? userData.CrewAbilityId
            : userData.ImposterAbilityId;

        return AbilityCollection.GetAbilityById(abilityId);
    }

    private AbilityDataSO GetRandomAbility(PlayerRole role)
    {
        return role == PlayerRole.CREWMATE
            ? AbilityCollection.GetRandomCrewMateAbility()
            : AbilityCollection.GetRandomImposterAbility();
    }
    private static void SpawnAbility(Player player, AbilityDataSO abilityDataSO)
    {
        if (abilityDataSO == null)
        {
            Debug.LogError("AbilityDataSO is null");
            return;
        }
        BaseAbility ability = Instantiate(abilityDataSO.AbilityPrefab).GetComponent<BaseAbility>();
        if (ability.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.SpawnWithOwnership(player.OwnerClientId);
            networkObject.transform.SetParent(player.transform);
        }
    }

}