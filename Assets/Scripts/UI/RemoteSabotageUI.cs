using System;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class RemoteSabotageUI : NetworkBehaviour
{
    //Attached to Player //Send Signal
    [field: SerializeField, Tooltip("Reference to the input system")]
    public InputReader InputReader { get; private set; }
    public static event Action SignalSabotageUIEvent;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        InputReader.OpenSabotageUIEvent += SignalToOpenSabotageDevice;
    }

    public void SignalToOpenSabotageDevice()
    {
        if (gameObject.GetComponent<Player>().Role.Value == PlayerRole.Imposter)
        {
            SignalSabotageUIEvent?.Invoke();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;
        InputReader.OpenSabotageUIEvent -= SignalToOpenSabotageDevice;
    }
}
