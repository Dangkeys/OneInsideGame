using System;
using UnityEngine;
using UnityEngine.Events;

public class VentTeleport : MonoBehaviour, IInteractable
{
    private bool firstVentEntry = true;
    private int ventID;
    public bool InVent = false;
    private Camera playerCamera;
    public UnityEvent OnInteractVent;

    void Start()
    {
        playerCamera = Camera.main;

        OnInteractVent.AddListener(GetComponentInParent<VentSystem>().InteractVent); //Subscriber
    }


    public void Interact(InteractionData interactionData)
    {
        if(firstVentEntry){
            firstVentEntry = false;
            ventID = Array.IndexOf(gameObject.GetComponentInParent<VentSystem>().VentsList, gameObject.transform);
            gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex = ventID;
        }
        
        if (interactionData.Interactor.TryGetComponent<Player>(out Player player))
        {
            if (!InVent)
            {
                InVent = true;
                playerCamera.enabled = false;
                gameObject.GetComponentInParent<VentSystem>().VentsList[gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex].GetComponentInChildren<Camera>().enabled = true;
                OnInteractVent.Invoke();
            }

            else if (InVent)
            {
                InVent = false;
                gameObject.GetComponentInParent<VentSystem>().VentsList[gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex].GetComponentInChildren<Camera>().enabled = false;
                playerCamera.enabled = true;
                OnInteractVent.Invoke();
                firstVentEntry = true;
            }

            // interactionData.Interactor.TryGetComponent<CharacterController>(out CharacterController cc);
            // cc.enabled = false;
            // player.transform.position = WarpPosition.position;
            // cc.enabled = true;
        }
    }


}
