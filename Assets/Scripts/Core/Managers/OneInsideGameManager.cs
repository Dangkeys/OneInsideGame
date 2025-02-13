
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;


public class OneInsideGameManager : SingletonPersistent<OneInsideGameManager>
{
    private CharacterManager characterManager;
    private PerkManager perkManager;
    private UserManager userManager;
    private AudioManager audioManager;
    private LobbyManager lobbyManager;
    private VoiceChatManager voiceChatManager;
    async void Start()
    {
        characterManager = GetComponentInChildren<CharacterManager>();
        perkManager = GetComponentInChildren<PerkManager>();
        userManager = GetComponentInChildren<UserManager>();
        audioManager = GetComponentInChildren<AudioManager>();
        voiceChatManager = GetComponentInChildren<VoiceChatManager>();
        lobbyManager = GetComponentInChildren<LobbyManager>();

        await UnityServices.InitializeAsync();
        await AuthenticationWrapper.DoAuth();

        Debug.Log("Player Name: " + AuthenticationService.Instance.PlayerName);

        await VivoxService.Instance.InitializeAsync();

        await VivoxService.Instance.LoginAsync();

        Loader.Load(GameScene.MainMenuScene);
    }
}
