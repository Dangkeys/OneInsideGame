using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class AbilityAssignment : NetworkBehaviour
{
    [field: SerializeField] public AbilityCollectionSO AbilityCollection { get; private set; }
    private OneInsideLevelSystem oneInsideLevelSystem;
    private RoleSystem roleSystem;
    private NetworkPlayerData networkPlayerData;
    private OneInsideGameManager oneInsideGameManager;
    public override void OnNetworkSpawn()
    {

        oneInsideLevelSystem = OneInsideLevelSystem.Instance;
        roleSystem = oneInsideLevelSystem.RoleSystem;
        oneInsideGameManager = OneInsideGameManager.Instance;
        if (!oneInsideGameManager)
            return;
        networkPlayerData = ConnectionManager.Instance.NetworkPlayerData;
        if (IsServer)
        {
            roleSystem.OnRolesAssignmentComplete += AssignAbilities;
        }
    }



    private void AssignAbilities()
    {
        var players = PlayerSystem.GetAllPlayer(null);
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
                bool hasValidAbilityId = role == PlayerRole.Crewmate
                    ? !string.IsNullOrEmpty(userData.CrewMateAbilityId)
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
        var abilityId = role == PlayerRole.Crewmate
            ? userData.CrewMateAbilityId
            : userData.ImposterAbilityId;

        return AbilityCollection.GetAbilityById(abilityId);
    }

    private AbilityDataSO GetRandomAbility(PlayerRole role)
    {
        return role == PlayerRole.Crewmate
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