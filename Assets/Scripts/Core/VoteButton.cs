using UnityEngine;

public class VoteButton : MonoBehaviour, IInteractable
{
    public void Interact(InteractionData interactionData)
    {
        if(!OneInsideLevelManager.Instance) return;
        OneInsideLevelManager.Instance.VoteManager.RaiseVoteStartServerRPC();
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
