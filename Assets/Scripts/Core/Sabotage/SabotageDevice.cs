using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SabotageDevice : MonoBehaviour, IInteractable
{
    private bool isSabotaging;
    public GameObject SabotageUI;
    public void Interact(InteractionData interactionData)
    {
        if (interactionData.Interactor.TryGetComponent<Player>(out Player player))
        {
            if (player.Role.Value == PlayerRole.Imposter)
            {
                if (!isSabotaging)
                {
                    isSabotaging = true;
                    SabotageUI.SetActive(true);
                }
                else
                {
                    isSabotaging = false;
                    SabotageUI.SetActive(false);
                }
            }
        }
    }

    private void ActivateSabotageUI(){
        
    }
}
