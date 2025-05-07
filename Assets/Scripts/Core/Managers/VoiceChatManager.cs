using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using UnityEngine;

public class VoiceChatManager : Singleton<VoiceChatManager>
{

    [field: SerializeField]
    List<RosterItem> rosterList = new List<RosterItem>();

    public async Task InitializeAsync()
    {
        await VivoxService.Instance.InitializeAsync();
        BindSessionEvents(true);
    }

    public static async Task LoginAsync()
    {
        LoginOptions options = new LoginOptions();
        options.DisplayName = AuthenticationService.Instance.PlayerName;
        options.EnableTTS = true;
        await VivoxService.Instance.LoginAsync(options);
    }

    public static async void JoinEchoChannelAsync()
    {
        string testChannel = AuthenticationService.Instance.PlayerId;
        await VivoxService.Instance.JoinEchoChannelAsync(testChannel, ChatCapability.TextAndAudio);
    }

    public static async void LeaveEchoChannelAsync()
    {
        string channelToLeave = AuthenticationService.Instance.PlayerId;
        await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
    }

    public static async void JoinPositionalChannelAsync(string channelName)
    {
        ChannelOptions channelOptions = new ChannelOptions();
        Channel3DProperties channel3DProperties = new Channel3DProperties();
        await VivoxService.Instance.JoinPositionalChannelAsync(channelName, ChatCapability.TextAndAudio, channel3DProperties, channelOptions);
    }

    public static async void LeavePositionalChannelAsync(string channelName)
    {
        await VivoxService.Instance.LeaveChannelAsync(channelName);
    }


    public static async void JoinGroupChannelAsync(string channelName)
    {
        await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.TextAndAudio);
    }

    public static async void LeaveGroupChannelAsync(string channelName)
    {
        await VivoxService.Instance.LeaveChannelAsync(channelName);
    }

    public static async void LogoutOfVivoxAsync()
    {
        await VivoxService.Instance.LogoutAsync();
    }
    private void BindSessionEvents(bool doBind)
    {
        if (doBind)
        {
            VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAddedToChannel;
            VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemovedFromChannel;
            VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
        }
        else
        {
            VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAddedToChannel;
            VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemovedFromChannel;
            VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
        }
    }

    private void OnChannelMessageReceived(VivoxMessage message)
    {

        string messageText = message.MessageText;
        string senderID = message.SenderPlayerId;
        string senderDisplayName = message.SenderDisplayName;
        string messageChannel = message.ChannelName;
        //TODO add UI
    }
    private void OnParticipantAddedToChannel(VivoxParticipant participant)
    {
        ///RosterItem is a class intended to store the participant object, and reflect events relating to it into the game's UI.
        ///It is a sample of one way to use these events, and is detailed just below this snippet.
        RosterItem newRosterItem = new RosterItem();
        newRosterItem.SetupRosterItem(participant);
        rosterList.Add(newRosterItem);
    }

    private void OnParticipantRemovedFromChannel(VivoxParticipant participant)
    {
        RosterItem rosterItemToRemove = rosterList.FirstOrDefault(p => p.Participant.PlayerId == participant.PlayerId);
        rosterList.Remove(rosterItemToRemove);
    }

    public static async Task MuteYourself()
    {
        await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.None, AuthenticationService.Instance.PlayerId);
    }
    public static void MutePlayerLocally(string playerId, string channelName)
    {
        VivoxService.Instance.ActiveChannels[channelName].Where(participant => participant.PlayerId == playerId).First().MutePlayerLocally();
    }

    public static void UnmutePlayerLocally(string playerId, string channelName)
    {
        VivoxService.Instance.ActiveChannels[channelName].Where(participant => participant.PlayerId == playerId).First().UnmutePlayerLocally();
    }
    public static async void LeaveAllChannels()
    {
        await VivoxService.Instance.LeaveAllChannelsAsync();
    }

    private void OnDestroy()
    {
        BindSessionEvents(false);
    }
}
