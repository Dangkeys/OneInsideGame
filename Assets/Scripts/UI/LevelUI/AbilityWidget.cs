using UnityEngine;
using UnityEngine.UI;

public class AbilityWidget : MonoBehaviour
{
    [SerializeField] private Image abilityIcon;
    [SerializeField] private PlayerSystem playerSystem;
    private BaseAbility ability;
    private Player player;
    private void Awake()
    {
        abilityIcon.type = Image.Type.Filled;
        abilityIcon.fillMethod = Image.FillMethod.Radial360;
        abilityIcon.fillOrigin = (int)Image.Origin360.Top;
        abilityIcon.fillClockwise = true;
        playerSystem.OnAllPlayersSpawnInTheGame += OnAllPlayersSpawnInTheGame;
    }

    private void OnAllPlayersSpawnInTheGame()
    {
        player = PlayerSystem.GetLocalPlayerScript();
        player.OnAbilityDataChanged += OnAbilityDataChanged;
    }

    private void OnAbilityDataChanged()
    {
        if (ability != null)
        {
            ability.ActiveTimer.TimeRemaining.OnValueChanged -= OnActiveTimerChanged;
            ability.CooldownTimer.TimeRemaining.OnValueChanged -= OnCooldownTimerChanged;
            ability.CurrentState.OnValueChanged -= OnAbilityStateChanged;
        }

        ability = player.AbilityData.AbilityPrefab.GetComponent<BaseAbility>();

        if (ability != null)
        {
            abilityIcon.sprite = player.AbilityData.Icon;
            ability.ActiveTimer.TimeRemaining.OnValueChanged += OnActiveTimerChanged;
            ability.CooldownTimer.TimeRemaining.OnValueChanged += OnCooldownTimerChanged;
            ability.CurrentState.OnValueChanged += OnAbilityStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (ability != null)
        {
            ability.ActiveTimer.TimeRemaining.OnValueChanged -= OnActiveTimerChanged;
            ability.CooldownTimer.TimeRemaining.OnValueChanged -= OnCooldownTimerChanged;
            ability.CurrentState.OnValueChanged -= OnAbilityStateChanged;
        }
        playerSystem.OnAllPlayersSpawnInTheGame -= OnAllPlayersSpawnInTheGame;
    }

    private void OnAbilityStateChanged(AbilityState previousValue, AbilityState newValue)
    {
        switch (newValue)
        {
            case AbilityState.Active:
                break;
            case AbilityState.Cooldown:
                abilityIcon.color = Color.gray;
                break;
            case AbilityState.ReadyToActivate:
                abilityIcon.color = Color.white;
                break;
        }
    }

    private void OnActiveTimerChanged(float previousValue, float newValue)
    {
        abilityIcon.fillAmount = ability.ActiveTimer.CurrentTime / ability.ActiveDuration;
    }

    private void OnCooldownTimerChanged(float previousValue, float newValue)
    {
        abilityIcon.fillAmount = (ability.CooldownDuration - ability.CooldownTimer.CurrentTime) / ability.CooldownDuration;
    }
}