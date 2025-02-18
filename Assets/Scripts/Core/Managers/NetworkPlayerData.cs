using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


public class NetworkPlayerData : NetworkBehaviour
{
    public NetworkVariable<Dictionary<ulong, FixedString32Bytes>> ClientIdToAuth { get; private set;} = 
        new NetworkVariable<Dictionary<ulong, FixedString32Bytes>>(new Dictionary<ulong, FixedString32Bytes>());
    public NetworkVariable<Dictionary<FixedString32Bytes, UserDataDto>> AuthIdToUserData { get; private set;} = 
        new NetworkVariable<Dictionary<FixedString32Bytes, UserDataDto>>(new Dictionary<FixedString32Bytes, UserDataDto>());

    
}