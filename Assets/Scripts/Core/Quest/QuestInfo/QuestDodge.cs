using Unity.Netcode;
using UnityEngine;

public class QuestDodge : QuestInfo, IInteractable
{
    [SerializeField] private QuestDodgeManager questDodgeManager;
    [SerializeField] private GameObject questDodgeUI;

    private void OnEnable()
    {
        questDodgeManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questDodgeManager.OnFinishedQuest -= Finished;
    }

    private void Finished(bool finished)
    {
        FinishQuest();
        CancelQuest();
        HandleFinishedServerRpc(finished);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleFinishedServerRpc(bool finished)
    {
        
        if (!IsHost)
        {
            HandleFinished(finished);
        }
        HandleFinishedClientRpc(finished);
    }

    [ClientRpc]
    private void HandleFinishedClientRpc(bool finished)
    {
        HandleFinished(finished);
    }

    private void HandleFinished(bool finished)
    {
        questDodgeUI.SetActive(false);
    }

    public void Interact(InteractionData interactionData)
    {
        if (!currentStatus)
        {
            UpdateDoQuest(true);
            questDodgeUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        UpdateDoQuest(false);
    }
}
