using System;
using UnityEngine;
using UnityEngine.Events;

public class VentTeleport : MonoBehaviour, IInteractable
{
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
        ventID = Array.IndexOf(gameObject.GetComponentInParent<VentSystem>().VentsList, gameObject.transform);
        gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex = ventID;

        if (interactionData.Interactor.TryGetComponent<Player>(out Player player))
        {
            if (!InVent)
            {
                InVent = true;
                playerCamera.enabled = false;
                gameObject.GetComponentInChildren<Camera>().enabled = true;
                OnInteractVent.Invoke();
            }

            else if (InVent)
            {
                InVent = false;
                playerCamera.enabled = true;
                gameObject.GetComponentInChildren<Camera>().enabled = false;
                OnInteractVent.Invoke();
            }

            // interactionData.Interactor.TryGetComponent<CharacterController>(out CharacterController cc);
            // cc.enabled = false;
            // player.transform.position = WarpPosition.position;
            // cc.enabled = true;
        }
    }


}
