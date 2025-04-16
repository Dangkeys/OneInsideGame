using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using OneInside.Utils.CloudSave;

public class UserDataManager : Singleton<UserDataManager>
{
    [field: SerializeField] public AbilityCollectionSO AbilityCollection { get; private set; }
    [field: SerializeField] public string CrewMateAbilityId { get; private set; } = "DefaultCrewMateAbilityId";
    [field: SerializeField] public string ImposterAbilityId { get; private set; } = "DefaultImposterAbilityId";

    private async void Start()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        _ = await AuthenticationWrapper.DoAuth();
        
        await LoadAbilitiesFromCloudSave();
        var saveTasks = new List<Task>();
    
        if(AbilityCollection.GetAbilityById(CrewMateAbilityId) == null)
        {
            CrewMateAbilityId = AbilityCollection.GetRandomCrewMateAbility().Id;
            saveTasks.Add(SaveAbilitiesToCloudSave(crewMateAbilityId: CrewMateAbilityId));
        }
        if(AbilityCollection.GetAbilityById(ImposterAbilityId) == null)
        {
            ImposterAbilityId = AbilityCollection.GetRandomImposterAbility().Id;
            saveTasks.Add(SaveAbilitiesToCloudSave(imposterAbilityId: ImposterAbilityId));
        }

        if (saveTasks.Count > 0)
        {
            await Task.WhenAll(saveTasks);
        }
    }
    public async Task SaveAbilitiesToCloudSave(string crewMateAbilityId = default, string imposterAbilityId = default)
    {
        try
        {
            if (!string.IsNullOrEmpty(crewMateAbilityId))
            {
                CrewMateAbilityId = crewMateAbilityId;
                await CloudSaveWrapper.SaveData(OneInside.Constants.CloudSave.CREWMATE_ABILITY_KEY, crewMateAbilityId);
            }
            if (!string.IsNullOrEmpty(imposterAbilityId))
            {
                ImposterAbilityId = imposterAbilityId;
                await CloudSaveWrapper.SaveData(OneInside.Constants.CloudSave.IMPOSTER_ABILITY_KEY, imposterAbilityId);
            }
            Debug.Log("Abilities saved to Cloud Save");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving abilities to Cloud Save: {e.Message}");
        }
    }

    public async Task LoadAbilitiesFromCloudSave()
    {
        try
        {
            CrewMateAbilityId = await CloudSaveWrapper.LoadData<string>(OneInside.Constants.CloudSave.CREWMATE_ABILITY_KEY) ?? CrewMateAbilityId;
            ImposterAbilityId = await CloudSaveWrapper.LoadData<string>(OneInside.Constants.CloudSave.IMPOSTER_ABILITY_KEY) ?? ImposterAbilityId;
            Debug.Log("Abilities loaded from Cloud Save");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading abilities from Cloud Save: {e.Message}");
        }
    }

    public UserDataDto GetUserData()
    {
        var userData = new UserDataDto();
        userData.AuthId = AuthenticationService.Instance.PlayerId;
        userData.Name = AuthenticationService.Instance.PlayerName;

        userData.CrewMateAbilityId = CrewMateAbilityId;
        userData.ImposterAbilityId = ImposterAbilityId;

        return userData;
    }
}