using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OneInsideGameManager : SingletonPersistent<OneInsideGameManager>
{
    public event Action<float, string> OnBootstrapLoadingProgressChanged;

    private readonly string[] loadingSteps = new[]
    {
        "Initializing Unity Services",
        "Authenticating User",
        "Generated Player Name",
        "Initializing Vivox Service",
        "Logging into Vivox Service",
        "Loading Main Menu"
    };


    private CharacterManager characterManager;
    private PerkManager perkManager;
    private UserManager userManager;
    private AudioManager audioManager;
    private LobbyManager lobbyManager;
    private VoiceChatManager voiceChatManager;

    private void UpdateProgress(int currentStep)
    {
        float progress = Mathf.Clamp((float)currentStep / loadingSteps.Length, 0, 1);
        OnBootstrapLoadingProgressChanged?.Invoke(progress, loadingSteps[currentStep]);
    }

    async void Start()
    {
        characterManager = GetComponentInChildren<CharacterManager>();
        perkManager = GetComponentInChildren<PerkManager>();
        userManager = GetComponentInChildren<UserManager>();
        audioManager = GetComponentInChildren<AudioManager>();
        voiceChatManager = GetComponentInChildren<VoiceChatManager>();
        lobbyManager = GetComponentInChildren<LobbyManager>();

        UpdateProgress(0);
        await UnityServices.InitializeAsync();

        UpdateProgress(1);
        AuthState state = await AuthenticationWrapper.DoAuth();

        if (state != AuthState.Authenticated)
        {
            //TODO Add logic to handle failed authentication
            Debug.LogError("Failed to authenticate user");
            return;
        }

        if (AuthenticationService.Instance.PlayerName == null)
        {
            UpdateProgress(2);
            await PlayerNameGenerator.GenerateRandomPlayerName();
        }

        UpdateProgress(3);
        await VivoxService.Instance.InitializeAsync();

        UpdateProgress(4);
        await VivoxService.Instance.LoginAsync();

        UpdateProgress(5);
        SceneManager.LoadScene(GameScene.MainMenuScene.ToString());
    }
}