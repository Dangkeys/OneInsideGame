using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class OneInsideGameManager : SingletonPersistent<OneInsideGameManager>
{
    public event Action<float, string> OnBootstrapLoadingProgressChanged;

    private readonly string[] loadingSteps = new[]
    {
        "Initializing Unity Services",
        "Authenticating User",
        "Initializing Vivox Service",
        "Logging into Vivox Service",
        "Loading Main Menu"
    };

    private int currentStep = 0;

    private CharacterManager characterManager;
    private PerkManager perkManager;
    private UserManager userManager;
    private AudioManager audioManager;
    private LobbyManager lobbyManager;
    private VoiceChatManager voiceChatManager;

    private void UpdateProgress(string status)
    {
        float progress = (float)currentStep / loadingSteps.Length;
        OnBootstrapLoadingProgressChanged?.Invoke(progress, status);
        currentStep++;
    }

    async void Start()
    {
        characterManager = GetComponentInChildren<CharacterManager>();
        perkManager = GetComponentInChildren<PerkManager>();
        userManager = GetComponentInChildren<UserManager>();
        audioManager = GetComponentInChildren<AudioManager>();
        voiceChatManager = GetComponentInChildren<VoiceChatManager>();
        lobbyManager = GetComponentInChildren<LobbyManager>();

        UpdateProgress(loadingSteps[0]);
        await UnityServices.InitializeAsync();

        UpdateProgress(loadingSteps[1]);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Player Name: " + AuthenticationService.Instance.PlayerName);

        UpdateProgress(loadingSteps[2]);
        await VivoxService.Instance.InitializeAsync();

        UpdateProgress(loadingSteps[3]);
        await VivoxService.Instance.LoginAsync();

        UpdateProgress(loadingSteps[4]);
        Loader.Load(GameScene.MainMenuScene);
    }
}