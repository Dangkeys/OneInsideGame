using Unity.Netcode;
using UnityEngine;

public class SlideDoor : NetworkBehaviour, IInteractable
{
    private bool doorOpen;
    public Animator DoorAnim;
    public void Interact(InteractionData interactionData)
    {   
        SlideDoorServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SlideDoorServerRPC(){
        if(!doorOpen){
            DoorAnim.SetTrigger("DoorOpen");
            doorOpen = true;
        }
        else if(doorOpen){
            DoorAnim.SetTrigger("DoorClose");
            doorOpen = false;
        }
    }
    
}
