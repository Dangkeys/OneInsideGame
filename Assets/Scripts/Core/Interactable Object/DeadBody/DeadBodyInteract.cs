using Unity.Netcode;
using UnityEngine;

public class DeadBodyInteract : NetworkBehaviour, IInteractable
{
    [field: SerializeField] public GameObject DeadBody { get; private set; }

    public void Interact(InteractionData interactionData)
    {
        DeadBody.GetComponent<DeadBody>().OnInteract(interactionData);
    }

    public bool CanInteract(InteractionData interactionData)
    {
        if (interactionData.InteractorGameObject.TryGetComponent<Player>(out Player player))
        {
            return player.IsAlive.Value;
        }
        return false;
    }
}
