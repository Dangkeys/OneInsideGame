using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class SabotageDevice : NetworkBehaviour
{
    //Attached to UI in Canvas //Subscriber
    private bool isSabotaging;

    private void Start()
    {
        RemoteSabotageUI.SignalSabotageUIEvent += OpenSabotageDevice;
        gameObject.SetActive(false);
    }

    private void OpenSabotageDevice()
    {
        if (!isSabotaging)
        {
            isSabotaging = true;
            gameObject.SetActive(true);
        }
        else
        {
            isSabotaging = false;
            gameObject.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        RemoteSabotageUI.SignalSabotageUIEvent -= OpenSabotageDevice;
    }

}
