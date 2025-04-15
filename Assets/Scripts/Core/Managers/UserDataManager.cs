using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

public class UserDataManager : Singleton<UserDataManager>
{
    [field: SerializeField] public string CrewMateAbilityId { get; private set; } = "DefaultCrewMateAbilityId";
    [field: SerializeField] public string ImposterAbilityId { get; private set; } = "DefaultImposterAbilityId";

    private async void Start()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }
        await UnityServices.InitializeAsync();

        _= await AuthenticationWrapper.DoAuth();
        _ = LoadAbilitiesFromCloudSave();
    }

    public async Task SaveAbilitiesToCloudSave()
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { OneInside.Constants.CloudSave.CREWMATE_ABILITY_KEY, "ABILITY_3658F5_992900" },
                { OneInside.Constants.CloudSave.IMPOSTER_ABILITY_KEY, "ABILITY_01DEA3_113327" }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
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
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { OneInside.Constants.CloudSave.CREWMATE_ABILITY_KEY, OneInside.Constants.CloudSave.IMPOSTER_ABILITY_KEY });

            if (playerData.TryGetValue(OneInside.Constants.CloudSave.CREWMATE_ABILITY_KEY, out var crewmateValue))
            {
                CrewMateAbilityId = crewmateValue.Value.GetAs<string>();
            }

            if (playerData.TryGetValue(OneInside.Constants.CloudSave.IMPOSTER_ABILITY_KEY, out var impostorValue))
            {
                ImposterAbilityId = impostorValue.Value.GetAs<string>();
            }

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
    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            await SaveAbilitiesToCloudSave();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Loading abilities from Cloud Save");
            await LoadAbilitiesFromCloudSave();
            Debug.Log("CrewMateAbilityId: " + CrewMateAbilityId);
            Debug.Log("ImposterAbilityId: " + ImposterAbilityId);
        }
    }
}