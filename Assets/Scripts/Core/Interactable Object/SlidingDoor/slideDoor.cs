using Unity.Netcode;
using UnityEngine;

public class slideDoor : NetworkBehaviour, IInteractable
{
    private bool doorOpen;
    public Animator doorAnim;
    public void Interact()
    {   
        SlideDoorServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SlideDoorServerRPC(){
        if(!doorOpen){
            //doorAnim.Play("SlideOpen", 0, 0.0f);
            doorAnim.SetTrigger("DoorOpen");
            doorOpen = true;
            
        }
        else if(doorOpen){
            //doorAnim.Play("SlideClose", 0, 0.0f);
            doorAnim.SetTrigger("DoorClose");
            doorOpen = false;
            
        }
    }
    
}
