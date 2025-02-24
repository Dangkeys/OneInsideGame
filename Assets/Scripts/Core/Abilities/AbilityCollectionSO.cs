using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityCollectionSO", menuName = "Scriptable Objects/Ability/AbilityCollectionSO", order = 0)]
public class AbilityCollectionSO : ScriptableObject
{
    [field: SerializeField] public List<AbilityDataSO> Abilities;

    public AbilityDataSO GetAbilityById(string id)
    {
        return Abilities.Find(ability => ability.Id == id);
    }

    public AbilityDataSO GetRandomAbility(AbilityType type)
    {
        var filteredAbilities = Abilities.FindAll(ability => ability.Type == type);

        if (filteredAbilities.Count == 0)
        {
            return null;
        }

        return filteredAbilities[Random.Range(0, filteredAbilities.Count)];
    }

    public AbilityDataSO GetRandomAbilityWithNeutral(AbilityType type)
    {
        if (Random.value < 0.7f)
        {
            var ability = GetRandomAbility(type);
            if (ability != null) return ability;
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