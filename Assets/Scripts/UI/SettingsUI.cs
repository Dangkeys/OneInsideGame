using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    private SettingsManager settingsManager;
    private void Awake()
    {
        settingsManager = OneInsideGameManager.Instance.SettingsManager;
        settingsManager.OnSettingsChanged += ToggleSettingsPanel;
    }

    private void ToggleSettingsPanel(bool isOpen)
    {
        gameObject.SetActive(isOpen);
    }
}