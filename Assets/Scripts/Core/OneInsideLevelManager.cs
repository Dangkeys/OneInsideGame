using System;
using Unity.Netcode;
using UnityEngine;

public class OneInsideLevelManager : NetworkBehaviour
{


    [field: SerializeField] public PlayerManager PlayerManager;
    [field: SerializeField] public VoteManager VoteManager;
    public static OneInsideLevelManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
}