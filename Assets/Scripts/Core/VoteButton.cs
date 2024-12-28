using UnityEngine;

public class VoteButton : MonoBehaviour, IInteractable
{
    public void Interact(InteractionData interactionData)
    {
        PlayerManager.Instance.OnSetAllPlayersToSpawnPosServerRPC();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
