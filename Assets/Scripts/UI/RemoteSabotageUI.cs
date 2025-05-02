using System;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class RemoteSabotageUI : NetworkBehaviour
{
    //Attached to Player //Send Signal
    private InputReader inputReader;
    public static event Action SignalSabotageUIEvent;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        inputReader = InputReader.Instance;
        inputReader.OpenSabotageUIEvent += SignalToOpenSabotageDevice;
    }

    public void SignalToOpenSabotageDevice()
    {
        //Debug.Log("Send Signal");
        if (gameObject.GetComponent<Player>().Role.Value == PlayerRole.Imposter)
        {
            SignalSabotageUIEvent?.Invoke();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;
        inputReader.OpenSabotageUIEvent -= SignalToOpenSabotageDevice;
    }
}
