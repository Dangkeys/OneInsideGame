using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class PlayerManager_Local : NetworkBehaviour
{

    public static NetworkObject GetLocalPlayer()
    {
        // foreach (Player player in GetAllPlayer())
        // {
        //     if (player.IsOwner)
        //     {
        //         return player.gameObject;
        //     }
        // }
        // return null;
        return NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;
    }

    public static List<Player> GetAllPlayer(Func<Player, bool> filter)
    {
        List<Player> playersObject = new List<Player>();
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject && client.PlayerObject.TryGetComponent<Player>(out var player))
                {
                    if (filter == null || filter(player))
                    {
                        playersObject.Add(player);
                    }
                }
            }
        }
        return playersObject;
    }

    public static List<Player> GetSpectatorPlayers(bool includeSelf = false)
    {
        return GetAllPlayer(player => player.GetComponent<PlayerState>().Spectator.Value && (includeSelf || !player.IsOwner));
    }

    // public static void UpdatePlayersVisible()
    // {
    //     NetworkObject LocalPlayer = GetLocalPlayer();
    //     if (LocalPlayer == null)
    //         return;

    //     bool CanSeeSpectator = LocalPlayer.GetComponent<PlayerState>().Spectator.Value;

    //     foreach (Player player in GetAllPlayer())
    //     {
    //         if (!player.IsOwner)
    //         {
    //             if (player.GetComponent<PlayerState>().Spectator.Value)
    //             {
    //                 player.gameObject.SetActive(CanSeeSpectator);
    //             }
    //             else
    //             {
    //                 player.gameObject.SetActive(true);
    //             }
    //         }
    //     }
    // }
}
