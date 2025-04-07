using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityCollectionSO", menuName = "Scriptable Objects/Ability/AbilityCollectionSO", order = 0)]
public class AbilityCollectionSO : ScriptableObject
{
    [field: SerializeField] public List<AbilityDataSO> Abilities;
    public List<AbilityDataSO> GetAllAbilties()
    {
        return Abilities;
    }

    public List<AbilityDataSO> GetAbilitiesByType(AbilityType type)
    {
        return Abilities.Where(ability => ability.Type == type).ToList();
    }

    public AbilityDataSO GetAbilityById(string abilityId)
    {
        return Abilities.FirstOrDefault(ability => ability.Id == abilityId);
    }

    public AbilityDataSO GetAbilityByName(string abilityName)
    {
        return Abilities.FirstOrDefault(ability => ability.Name == abilityName);
    }

    public List<AbilityDataSO> GetAbilitiesByKeyword(string keyword)
    {
        keyword = keyword.ToLower();
        return Abilities.Where(ability =>
            ability.Name.ToLower().Contains(keyword) ||
            ability.Description.ToLower().Contains(keyword)
        ).ToList();
    }

    public List<AbilityDataSO> QueryAbilities(Func<AbilityDataSO, bool> predicate)
    {
        return Abilities.Where(predicate).ToList();
    }


    public AbilityDataSO GetRandomAbility(AbilityType type)
    {
        var filteredAbilities = Abilities.FindAll(ability => ability.Type == type);

        if (filteredAbilities.Count == 0)
        {
            return null;
        }

        return filteredAbilities[UnityEngine.Random.Range(0, filteredAbilities.Count)];
    }

    public AbilityDataSO GetRandomAbilityWithNeutral(AbilityType type)
    {
        if (UnityEngine.Random.value < 0.7f)
        {
            var ability = GetRandomAbility(type);
            if (ability != null)
                return ability;
        }

        return GetRandomAbility(AbilityType.NEUTRAL);
    }

    public AbilityDataSO GetRandomCrewMateAbility()
    {
        return GetRandomAbilityWithNeutral(AbilityType.CREWMATE);
    }

    public AbilityDataSO GetRandomImposterAbility()
    {
        return GetRandomAbilityWithNeutral(AbilityType.IMPOSTER);
    }
}