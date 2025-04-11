using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GamePlayingTimerUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        if (OneInsideLevelSystem.Instance == null)
        {
            Debug.LogError(OneInside.Constants.OneInsideError.NULL);
            return;
        }
        OneInsideLevelSystem.Instance.GamePlayTimer.OnValueChanged += UpdateTimer;
        gameObject.SetActive(false);   
    }

    private void UpdateTimer(float previousValue, float newValue)
    {
        gameObject.SetActive(true);
        timerText.text = TimeSpan.FromSeconds(newValue).ToString(@"mm\:ss");
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
    }
}