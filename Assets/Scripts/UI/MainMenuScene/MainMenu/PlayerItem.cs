using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    [field: SerializeField] public TextMeshProUGUI PlayerNameText { get; private set; }
    [SerializeField] private Image readyIcon;
    private ulong playerId;
    private ReadyManager readyManager;

    private void Start()
    {
        InitializeReadyManager();
    }

    private void InitializeReadyManager()
    {
        if (readyManager == null)
        {
            readyManager = ReadyManager.Instance;
            if (readyManager != null)
            {
                readyManager.OnReadyStateChanged += OnReadyStateChanged;
            }
        }
    }

    private void OnDestroy()
    {
        if (readyManager != null)
        {
            readyManager.OnReadyStateChanged -= OnReadyStateChanged;
        }
    }

    private void OnReadyStateChanged(Dictionary<ulong, bool> newDict)
    {
        UpdateVisualState(newDict);
    }

    public void Initialize(string clientId)
    {
        playerId = ulong.Parse(clientId);
        PlayerNameText.text = clientId;
        InitializeReadyManager();

        if (readyManager != null)
        {
            UpdateVisualState(readyManager.GetCurrentReadyStates());
        }
    }

    private void UpdateVisualState(Dictionary<ulong, bool> readyStates)
    {
        if (readyStates == null)
            return;

        if (readyStates.TryGetValue(playerId, out bool isReady))
        {
            if (readyIcon != null)
            {
                readyIcon.gameObject.SetActive(isReady);
            }
        }
    }
}