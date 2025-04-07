using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityListItemUI : MonoBehaviour {
    private AbilityDataSO abilityDataSO;

    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityDescriptionText;

    public void SetAbilityData(AbilityDataSO abilityData) {
        abilityDataSO = abilityData;
        abilityIcon.sprite = abilityDataSO.Icon;
        abilityNameText.text = abilityDataSO.Name;
        abilityDescriptionText.text = abilityDataSO.Description;
    }

}