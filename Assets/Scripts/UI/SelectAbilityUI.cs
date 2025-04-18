using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SelectAbilityUI : MonoBehaviour
{
    [SerializeField] private AbilityListItemUI crewMateListItemUI;
    [SerializeField] private AbilityListItemUI imposterListItemUI;

    [SerializeField] private GameObject abilityItemContainer;
    [SerializeField] private AbilityListItemUI abilityListItemPrefab;
    private AbilityCollectionSO abilityCollection;
    private bool isChoosingCrewMate = true;

    private void Awake()
    {
        abilityCollection = UserDataManager.Instance.AbilityCollection;
    }

    private void Start()
    {
        crewMateListItemUI.Setup(abilityCollection.GetAbilityById(UserDataManager.Instance.CrewMateAbilityId), (_) => SwitchAbilitySelection(true)); 
        imposterListItemUI.Setup(abilityCollection.GetAbilityById(UserDataManager.Instance.ImposterAbilityId), (_) => SwitchAbilitySelection(false));


        // Initialize UI
        UpdateAbilityButtonIcons();
        ShowCrewMateAbilities();
    }

    /// <summary>
    /// Switches between crewmate and imposter ability selection
    /// </summary>
    private void SwitchAbilitySelection(bool isCrewMate)
    {
        isChoosingCrewMate = isCrewMate;
        if (isCrewMate)
        {
            ShowCrewMateAbilities();
        }
        else
        {
            ShowImposterAbilities();
        }
    }

    /// <summary>
    /// Updates the button icons based on currently selected abilities
    /// </summary>
    private void UpdateAbilityButtonIcons()
    {
        // Update crewmate button icon
        UpdateButtonIcon(
            UserDataManager.Instance.CrewMateAbilityId,
            crewMateListItemUI.GetComponent<Image>()
        );

        // Update imposter button icon
        UpdateButtonIcon(
            UserDataManager.Instance.ImposterAbilityId,
            imposterListItemUI.GetComponent<Image>()
        );
    }

    /// <summary>
    /// Updates a specific button icon with the ability icon
    /// </summary>
    private void UpdateButtonIcon(string abilityId, Image buttonIcon)
    {
        var ability = abilityCollection.Abilities.FirstOrDefault(a => a.Id == abilityId);
        if (ability != null)
        {
            buttonIcon.sprite = ability.Icon;
        }
    }

    /// <summary>
    /// Clears all ability items from the container
    /// </summary>
    private void ClearAbilityContainer()
    {
        foreach (Transform child in abilityItemContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Shows all crewmate and neutral abilities
    /// </summary>
    private void ShowCrewMateAbilities()
    {
        ShowAbilities(AbilityType.CREWMATE);
    }

    /// <summary>
    /// Shows all imposter and neutral abilities
    /// </summary>
    private void ShowImposterAbilities()
    {
        ShowAbilities(AbilityType.IMPOSTER);
    }

    /// <summary>
    /// Shows abilities of the specified type along with neutral abilities
    /// </summary>
    private void ShowAbilities(AbilityType primaryType)
    {
        ClearAbilityContainer();
        var abilities = abilityCollection.Abilities.Where(a => 
            a.Type == primaryType || a.Type == AbilityType.NEUTRAL);

        foreach (var ability in abilities)
        {
            var item = Instantiate(abilityListItemPrefab, abilityItemContainer.transform);
            item.Setup(ability, OnAbilitySelected);
        }
    }

    /// <summary>
    /// Handles ability selection and saves the choice
    /// </summary>
    private async void OnAbilitySelected(string abilityId)
    {
        var selectedAbility = abilityCollection.Abilities.FirstOrDefault(a => a.Id == abilityId);
        if (selectedAbility == null) return;

        // Validate ability type matches the current selection mode
        AbilityType requiredType = isChoosingCrewMate ? AbilityType.CREWMATE : AbilityType.IMPOSTER;
        if (selectedAbility.Type != requiredType && selectedAbility.Type != AbilityType.NEUTRAL)
        {
            Debug.LogWarning($"Invalid ability type selected for {(isChoosingCrewMate ? "CrewMate" : "Imposter")}");
            return;
        }

        // Save the selected ability
        if (isChoosingCrewMate)
        {
            await UserDataManager.Instance.SaveAbilitiesToCloudSave(crewMateAbilityId: abilityId);
        }
        else
        {
            await UserDataManager.Instance.SaveAbilitiesToCloudSave(imposterAbilityId: abilityId);
        }

        UpdateAbilityButtonIcons();
    }
}
