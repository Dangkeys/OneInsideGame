using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonPersistent<UIManager>
{

    public event Action<float, string> OnLoadingProgressChanged;
    public event Action<string, Action, Action> OnConfirmationRequired;
    public event Action<string> OnShowMessageRequired;

    private readonly Dictionary<OneInsideGameManager.GameEvent, float> gameEventProgressMapping;

    private readonly Dictionary<string, List<OneInsideGameManager.GameEvent>> progressSequences;
    

    private string currentSequence;

    
    private OneInsideGameManager gameManager;

    public UIManager()
    {
        // Initialize progress mapping for game events
        gameEventProgressMapping = new Dictionary<OneInsideGameManager.GameEvent, float>
        {

            { OneInsideGameManager.GameEvent.InitializingServices, 0f },
            { OneInsideGameManager.GameEvent.AuthenticatingUser, 0.16f },
            { OneInsideGameManager.GameEvent.GeneratingPlayerName, 0.33f },
            { OneInsideGameManager.GameEvent.InitializingVivox, 0.5f },
            { OneInsideGameManager.GameEvent.LoggingIntoVivox, 0.66f }, 
            { OneInsideGameManager.GameEvent.LoadingMainMenu, 0.83f },
            { OneInsideGameManager.GameEvent.GameInitialized, 1f },
            
            // Host/Join match sequence - 3 steps
            { OneInsideGameManager.GameEvent.CreatingLobby, 0f },
            { OneInsideGameManager.GameEvent.JoiningLobby, 0f },
            { OneInsideGameManager.GameEvent.LoadingLobbyScene, 0.5f },
            { OneInsideGameManager.GameEvent.LobbySceneLoaded, 1f },
            
            // Game start/stop sequence - special cases
            { OneInsideGameManager.GameEvent.StartingGame, 0.5f },
            { OneInsideGameManager.GameEvent.GameStarted, 1f },
            { OneInsideGameManager.GameEvent.StoppingGame, 0.5f },
            { OneInsideGameManager.GameEvent.GameStopped, 1f }
        };
        

        progressSequences = new Dictionary<string, List<OneInsideGameManager.GameEvent>>
        {
            { "GameInitialization", new List<OneInsideGameManager.GameEvent> 
                { 
                    OneInsideGameManager.GameEvent.InitializingServices,
                    OneInsideGameManager.GameEvent.AuthenticatingUser,
                    OneInsideGameManager.GameEvent.GeneratingPlayerName,
                    OneInsideGameManager.GameEvent.InitializingVivox,
                    OneInsideGameManager.GameEvent.LoggingIntoVivox,
                    OneInsideGameManager.GameEvent.LoadingMainMenu,
                    OneInsideGameManager.GameEvent.GameInitialized
                }
            },
            { "HostMatch", new List<OneInsideGameManager.GameEvent>
                {
                    OneInsideGameManager.GameEvent.CreatingLobby,
                    OneInsideGameManager.GameEvent.LoadingLobbyScene,
                    OneInsideGameManager.GameEvent.LobbySceneLoaded
                }
            },
            { "JoinMatch", new List<OneInsideGameManager.GameEvent>
                {
                    OneInsideGameManager.GameEvent.JoiningLobby,
                    OneInsideGameManager.GameEvent.LoadingLobbyScene,
                    OneInsideGameManager.GameEvent.LobbySceneLoaded
                }
            }
        };
    }

    protected override void OnAwakeInitialization()
    {
        base.OnAwakeInitialization();

        gameManager = OneInsideGameManager.Instance;
        
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += HandleGameStateChanged;
        }
        else
        {
            Debug.LogError("Game Manager not found!");
        }
    }

    private void HandleGameStateChanged(OneInsideGameManager.GameEvent gameEvent, string message)
    {
        if (gameEvent == OneInsideGameManager.GameEvent.OperationFailed)
        {
            ShowMessage(message);
            return;
        }
        
        foreach (var sequence in progressSequences)
        {
            if (sequence.Value.Contains(gameEvent))
            {
                // If starting a new sequence
                if (currentSequence != sequence.Key)
                {
                    currentSequence = sequence.Key;
                }

                float progress = gameEventProgressMapping[gameEvent];

                ShowProgressChanged(progress, message);
                break;
            }
        }
        
        // Handle special case events that aren't part of a sequence
        if (gameEvent == OneInsideGameManager.GameEvent.StartingGame ||
            gameEvent == OneInsideGameManager.GameEvent.GameStarted ||
            gameEvent == OneInsideGameManager.GameEvent.StoppingGame ||
            gameEvent == OneInsideGameManager.GameEvent.GameStopped)
        {
            float progress = gameEventProgressMapping[gameEvent];
            ShowProgressChanged(progress, message);
        }
    }

    public void ShowConfirmation(string message, Action onConfirm, Action onCancel = null)
    {
        OnConfirmationRequired?.Invoke(message, onConfirm, onCancel);
    }

    public void ShowMessage(string message)
    {
        OnLoadingProgressChanged?.Invoke(1, message);
        OnShowMessageRequired?.Invoke(message);
    }

    public void ShowProgressChanged(float progress, string message)
    {
        OnLoadingProgressChanged?.Invoke(progress, message);
    }
}