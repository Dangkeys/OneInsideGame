public interface IInteractable
{
    void Interact(InteractionData interactionData);

    // optional
    bool CanInteract(InteractionData interactionData) => true;
}
