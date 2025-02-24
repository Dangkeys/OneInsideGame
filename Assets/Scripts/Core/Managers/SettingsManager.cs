using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    private InputReader inputReader;

    private bool isSettingsOpen = false;
    public Action<bool> OnSettingsChanged;
    void Awake()
    {
        inputReader = OneInsideGameManager.Instance.InputReader;
        inputReader.EscapeEvent += ToggleSettings;
    }

    private void ToggleSettings()
    {
        isSettingsOpen = !isSettingsOpen;
        OnSettingsChanged?.Invoke(isSettingsOpen);
    }
}
