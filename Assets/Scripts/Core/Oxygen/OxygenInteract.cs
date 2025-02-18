using UnityEngine;

public class OxygenInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject oxygen;
    private PlayerMovement playerMovement;
    public void Interact(InteractionData interactionData)
    {
        if (!playerMovement)
        {
            playerMovement = interactionData.Interactor.GetComponent<PlayerMovement>();
        }
        if(playerMovement)
        {
            playerMovement.EnablePlayerMovement(false);
        }
        oxygen.SetActive(true);
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
