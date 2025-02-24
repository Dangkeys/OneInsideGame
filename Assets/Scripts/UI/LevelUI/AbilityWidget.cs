using System;
using Mono.CSharp;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AbilityWidget : MonoBehaviour
{
    private PlayerManager playerManager;
    private Player player;
    [SerializeField] private Image backgroundIcon;
    [SerializeField] private Image abilityIcon;
    private BaseAbility ability;

    private float maxActiveTime;
    private float maxCooldown;

    void Awake()
    {
        playerManager = OneInsideLevelManager.Instance.PlayerManager;
        SetupIcon(backgroundIcon);
        SetupIcon(abilityIcon);
    }

    private void SetupIcon(Image icon)
    {
        if (icon != null)
        {
            icon.type = Image.Type.Filled;
            icon.fillMethod = Image.FillMethod.Radial360;
            icon.fillOrigin = (int)Image.Origin360.Top;
            icon.fillClockwise = true;  // Both icons fill clockwise
        }
    }

    private void Start()
    {
        playerManager.OnAllPlayersSpawnInTheGame += OnAllPlayersSpawnInTheGame;

    }



    private void OnAllPlayersSpawnInTheGame()
    {
        player = PlayerManager.GetLocalPlayerScript();
        if (player != null)
        {
            player.OnAbilityDataChanged += OnAbilityDataChanged;
        }
        else
        {
            Debug.LogWarning("Player is null in AbilityWidget!");
        }
    }

    private void OnAbilityDataChanged()
    {
        abilityIcon.sprite = player.AbilityData.Icon;
        ability = player.GetComponentInChildren<BaseAbility>();

        if (ability != null)
        {
            maxActiveTime = ability.ActiveTime;
            maxCooldown = ability.CooldownTime;

            ability.State.OnValueChanged += StateChanged;
            ability.RemainingActiveTime.OnValueChanged += OnRemainingActiveTimeChanged;
            ability.RemainingCooldown.OnValueChanged += OnRemainingCooldownChanged;
        }
    }



    private void StateChanged(AbilityState previousValue, AbilityState newValue)
    {
        switch (newValue)
        {
            case AbilityState.READY:
                abilityIcon.fillAmount = 1;
                backgroundIcon.fillAmount = 1;
                break;
            case AbilityState.ACTIVE:
                break;
            case AbilityState.COOLDOWN:
                abilityIcon.color = Color.gray;
                backgroundIcon.color = Color.gray;
                break;

        }
    }

    private void OnRemainingActiveTimeChanged(float previousValue, float newValue)
    {
        Debug.Log(newValue);
        UpdateFill(backgroundIcon, newValue, maxActiveTime, true);
        UpdateFill(abilityIcon, newValue, maxActiveTime, true);
    }

    private void OnRemainingCooldownChanged(float previousValue, float newValue)
    {
        Debug.Log(newValue);
        UpdateFill(backgroundIcon, newValue, maxCooldown, false);
        UpdateFill(abilityIcon, newValue, maxCooldown, false);
    }
    private void UpdateFill(Image icon, float currentValue, float maxValue, bool isActive)
    {
        if (icon != null && maxValue > 0)
        {
            float fillAmount = currentValue / maxValue;

            if (isActive)
            {
                icon.fillAmount = fillAmount;
            }
            else
            {
                icon.fillAmount = (currentValue <= 0) ? 1 : 1 - fillAmount;
            }
        }
    }


    private void UnsubscribeEvents()
    {
        if (player != null)
        {
            player.OnAbilityDataChanged -= OnAbilityDataChanged;
        }

        if (ability != null)
        {
            ability.RemainingActiveTime.OnValueChanged -= OnRemainingActiveTimeChanged;
            ability.RemainingCooldown.OnValueChanged -= OnRemainingCooldownChanged;
        }

        playerManager.OnAllPlayersSpawnInTheGame -= OnAllPlayersSpawnInTheGame;
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }
}