using UnityEngine;
using UnityEngine.UI;

public class SabotageDevice : MonoBehaviour,IInteractable
{
    private bool isSabotaging;
    public GameObject SabotageUI;
    public void Interact(InteractionData interactionData)
    {
        if(!isSabotaging){
            isSabotaging = true;
            SabotageUI.SetActive(true);
        }
        else{
            isSabotaging = false;
            SabotageUI.SetActive(false);
        }
    }
}
