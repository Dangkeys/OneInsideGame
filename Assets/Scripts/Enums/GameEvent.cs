    public enum GameEvent
    {

        InitializingServices,
        AuthenticatingUser,
        GeneratingPlayerName,
        InitializingVivox,
        LoggingIntoVivox,
        LoadingMainMenu,
        GameInitialized,

        CreatingLobby,
        LoadingLobbyScene,
        LobbySceneLoaded,

        JoiningLobby,

        OperationFailed,
        StartingGame,
        GameStarted,
        StoppingGame,
        GameStopped
    }