using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class VentTeleport : MonoBehaviour, IInteractable
{
    //public Transform WarpPosition;
    bool inVent;
    private Camera playerCamera;

    void Start(){
        playerCamera = Camera.main;
    }


    public void Interact(InteractionData interactionData)
    {
        Debug.Log("Interact Vent");
        //VentCamera = gameObject.GetComponentInChildren<Camera>();
        if (interactionData.Interactor.TryGetComponent<Player>(out Player player))
        {   
            if (!inVent)
            {
                //player.enabled = false;
                Debug.Log("Get in Vent");
                playerCamera.enabled = false;
                gameObject.GetComponentInChildren<Camera>().enabled = true;
                inVent = true;
            }

            else if (inVent)
            {
                //player.enabled = true;
                Debug.Log("Get out Vent");
                playerCamera.enabled = true;
                gameObject.GetComponentInChildren<Camera>().enabled = false;
                inVent = false;
            }

            // interactionData.Interactor.TryGetComponent<CharacterController>(out CharacterController cc);
            // cc.enabled = false;
            // player.transform.position = WarpPosition.position;
            // cc.enabled = true;
        }
    }
}
