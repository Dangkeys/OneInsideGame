using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AbilityListItemUI : MonoBehaviour
{
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI abilityName;
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI abilityDescription;

    private AbilityDataSO ability;
    private Action<string> onAbilitySelected;

    public void Setup(AbilityDataSO ability, Action<string> onSelected)
    {
        this.ability = ability;
        onAbilitySelected = onSelected;

        abilityIcon.sprite = ability.Icon;
        abilityName.text = ability.Name;
        selectButton.onClick.AddListener(HandleSelection);
        abilityDescription.text = ability.Description;
    }

    private void HandleSelection()
    {
        onAbilitySelected?.Invoke(ability.Id);
    }

    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
