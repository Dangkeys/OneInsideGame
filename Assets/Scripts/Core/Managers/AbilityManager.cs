using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    [field: SerializeField] public AbilityCollectionSO AbilityCollection { get; private set; }
    public AbilityDataSO SelectedCrewAbility { get; private set; }

    public AbilityDataSO SelectedImposterAbility { get; private set; }


    async void Awake()
    {
        SelectedCrewAbility = AbilityCollection.GetRandomCrewMateAbility();
        SelectedImposterAbility = AbilityCollection.GetRandomImposterAbility();
        SelectedCrewAbility = await GetSelectedCrewAbilityData();
        SelectedImposterAbility = await GetSelectedImposterAbilityData();

    }

    public async void SetSelectedCrewAbilityById(string abilityId)
    {
        AbilityDataSO abilityDataSO = AbilityCollection.GetAbilityById(abilityId);
        if (abilityDataSO == null)
        {
            Debug.LogError($"Ability with ID {abilityId} not found in AbilityCollection.");
            return;
        }
        if (abilityDataSO.Type == AbilityType.IMPOSTER)
        {
            Debug.LogError($"Ability with ID {abilityId} is imposter ability.");
            return;
        }

        SelectedCrewAbility = abilityDataSO;
        await CloudSaveWrapper.ForceSaveSingleData(OneInside.Constants.CloudSave.KEY_SELECTED_CREW_ABILITY_ID, abilityId);
    }
    public async void SetSelectedImposterAbilityById(string abilityId)
    {
        AbilityDataSO abilityDataSO = AbilityCollection.GetAbilityById(abilityId);
        if (abilityDataSO == null)
        {
            Debug.LogError($"Ability with ID {abilityId} not found in AbilityCollection.");
            return;
        }
        if (abilityDataSO.Type == AbilityType.CREWMATE)
        {
            Debug.LogError($"Ability with ID {abilityId} is crewmate ability.");
            return;
        }

        SelectedImposterAbility = abilityDataSO;
        await CloudSaveWrapper.ForceSaveSingleData(OneInside.Constants.CloudSave.KEY_SELECTED_IMPOSTER_ABILITY_ID, abilityId);
    }

    public async Task<AbilityDataSO> GetSelectedCrewAbilityData()
    {
        string abilityId = await CloudSaveWrapper.RetrieveSpecificData<string>(OneInside.Constants.CloudSave.KEY_SELECTED_CREW_ABILITY_ID);
        return AbilityCollection.GetAbilityById(abilityId);
    }

    public async Task<AbilityDataSO> GetSelectedImposterAbilityData()
    {
        string abilityId = await CloudSaveWrapper.RetrieveSpecificData<string>(OneInside.Constants.CloudSave.KEY_SELECTED_IMPOSTER_ABILITY_ID);
        return AbilityCollection.GetAbilityById(abilityId);
    }
}
