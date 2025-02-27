using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;

public class SabotageDevice : NetworkBehaviour
{
    //Attached to UI in Canvas //Subscriber
    private bool isSabotaging;

    private void Start()
    {
        gameObject.SetActive(true);
        RemoteSabotageUI.SignalSabotageUIEvent += OpenSabotageDevice;
        gameObject.SetActive(false);
    }

    private void OpenSabotageDevice()
    {
        //Debug.Log("Received Signal");
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

    public override void OnDestroy(){
        RemoteSabotageUI.SignalSabotageUIEvent -= OpenSabotageDevice;
    }

}
