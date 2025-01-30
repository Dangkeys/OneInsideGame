using UnityEngine;

public class OxygenInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject oxygen;
    public void Interact(InteractionData interactionData)
    {
        oxygen.SetActive(true);
    }
}
