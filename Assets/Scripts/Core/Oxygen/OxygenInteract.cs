using UnityEngine;

public class OxygenInteract : MonoBehaviour, IInteractable
{
    public bool IsDisabled;
    [field: SerializeField] private GameObject oxygen; //Jui changed this to public
    private PlayerMovement playerMovement;
    public void Interact(InteractionData interactionData)
    {
        if (!IsDisabled)
        {
            if (!playerMovement)
            {
                playerMovement = interactionData.Interactor.GetComponent<PlayerMovement>();
            }
            if (playerMovement)
            {
                playerMovement.EnablePlayerMovement(false);
            }
            oxygen.SetActive(true);
        }

    }

    public void EnableMovement()
    {
        if (playerMovement)
        {
            playerMovement.EnablePlayerMovement(true);
        }
        oxygen.SetActive(false);
    }
}
