using System;
using TMPro;
using UnityEngine;

public class GamePlayingTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        if (OneInsideLevelManager.Instance == null)
        {
            Debug.LogError(OneInside.Constants.OneInsideError.NULL);
            return;
        }
        OneInsideLevelManager.Instance.GamePlayTimer.OnValueChanged += UpdateTimer;
        gameObject.SetActive(false);   
    }

    private void UpdateTimer(float previousValue, float newValue)
    {
        gameObject.SetActive(true);
        timerText.text = TimeSpan.FromSeconds(newValue).ToString(@"mm\:ss");
    }
}