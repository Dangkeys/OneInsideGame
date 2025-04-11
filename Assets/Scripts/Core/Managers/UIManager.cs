using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonPersistent<UIManager>
{

    public event Action<float, string> OnLoadingProgressChanged;
    public event Action<string, Action, Action> OnConfirmationRequired;
    public event Action<string> OnShowMessageRequired;

    private readonly Dictionary<GameEvent, float> gameEventProgressMapping;

    private readonly Dictionary<string, List<GameEvent>> progressSequences;
    

    private string currentSequence;

    
    private OneInsideGameManager gameManager;

    public UIManager()
    {
        // Initialize progress mapping for game events
        gameEventProgressMapping = new Dictionary<GameEvent, float>
        {

            { GameEvent.InitializingServices, 0f },
            { GameEvent.AuthenticatingUser, 0.16f },
            { GameEvent.GeneratingPlayerName, 0.33f },
            { GameEvent.InitializingVivox, 0.5f },
            { GameEvent.LoggingIntoVivox, 0.66f }, 
            { GameEvent.LoadingMainMenu, 0.83f },
            { GameEvent.GameInitialized, 1f },

            { GameEvent.CreatingLobby, 0f },
            { GameEvent.JoiningLobby, 0f },
            { GameEvent.LoadingLobbyScene, 0.5f },
            { GameEvent.LobbySceneLoaded, 1f },

            { GameEvent.StartingGame, 0.5f },
            { GameEvent.GameStarted, 1f },
            { GameEvent.StoppingGame, 0.5f },
            { GameEvent.GameStopped, 1f }
        };
        

        progressSequences = new Dictionary<string, List<GameEvent>>
        {
            { "GameInitialization", new List<GameEvent> 
                { 
                    GameEvent.InitializingServices,
                    GameEvent.AuthenticatingUser,
                    GameEvent.GeneratingPlayerName,
                    GameEvent.InitializingVivox,
                    GameEvent.LoggingIntoVivox,
                    GameEvent.LoadingMainMenu,
                    GameEvent.GameInitialized
                }
            },
            { "HostMatch", new List<GameEvent>
                {
                    GameEvent.CreatingLobby,
                    GameEvent.LoadingLobbyScene,
                    GameEvent.LobbySceneLoaded
                }
            },
            { "JoinMatch", new List<GameEvent>
                {
                    GameEvent.JoiningLobby,
                    GameEvent.LoadingLobbyScene,
                    GameEvent.LobbySceneLoaded
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

    private void HandleGameStateChanged(GameEvent gameEvent, string message)
    {
        if (gameEvent == GameEvent.OperationFailed)
        {
            ShowMessage(message);
            return;
        }
        
        foreach (var sequence in progressSequences)
        {
            if (sequence.Value.Contains(gameEvent))
            {

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
        if (gameEvent == GameEvent.StartingGame ||
            gameEvent == GameEvent.GameStarted ||
            gameEvent == GameEvent.StoppingGame ||
            gameEvent == GameEvent.GameStopped)
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