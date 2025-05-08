using Unity.Netcode;
using UnityEngine;

public class SlideDoor : NetworkBehaviour, IInteractable
{
    public bool IsDisabled;
    public bool DoorOpen;
    public Animator DoorAnim;
    public void Interact(InteractionData interactionData)
    {
        if (!IsDisabled)
        {
            SlideDoorServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SlideDoorServerRPC()
    {
        if (!DoorOpen)
        {
            DoorAnim.SetTrigger("Open");
            DoorOpen = true;
        }
        else if (DoorOpen)
        {
            DoorAnim.SetTrigger("Close");
            DoorOpen = false;
        }
    }

}
