using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class VentTeleport : MonoBehaviour, IInteractable
{
    public Transform WarpPosition;

    public void Interact(InteractionData interactionData)
    {
        if (interactionData.Interactor.TryGetComponent<Player>(out Player player))
        {
            interactionData.Interactor.TryGetComponent<CharacterController>(out CharacterController cc);
            cc.enabled = false;
            player.transform.position = WarpPosition.position;
            cc.enabled = true;
        }

    }
}
