using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class VentTeleport : MonoBehaviour, IInteractable
{
    //public Transform WarpPosition;
    bool inVent;

    public void Interact(InteractionData interactionData)
    {
        if (interactionData.Interactor.TryGetComponent<Player>(out Player player))
        {
            if (player.TryGetComponent<Camera>(out Camera playerCamera))
            {
                if (!inVent)
                {
                    playerCamera.enabled = false;
                    if (gameObject.TryGetComponent<Camera>(out Camera ventCamera))
                    {
                        ventCamera.enabled = true;
                        inVent = true;
                    }
                }

                if (inVent)
                {
                    playerCamera.enabled = true;
                    if (gameObject.TryGetComponent<Camera>(out Camera ventCamera))
                    {
                        ventCamera.enabled = false;
                        inVent = false;
                    }
                }
            }


            // interactionData.Interactor.TryGetComponent<CharacterController>(out CharacterController cc);
            // cc.enabled = false;
            // player.transform.position = WarpPosition.position;
            // cc.enabled = true;
        }
    }
}
