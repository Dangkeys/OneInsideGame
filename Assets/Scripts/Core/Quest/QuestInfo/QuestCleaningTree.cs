using Unity.Netcode;
using UnityEngine;

public class QuestCleaningTree : QuestInfo, IInteractable
{
    [SerializeField] private QuestCleaningTreeManager questCleaningTreeManager;
    [SerializeField] private GameObject questCleaningUI;

    private void OnEnable()
    {
        questCleaningTreeManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questCleaningTreeManager.OnFinishedQuest -= Finished;
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
        questCleaningUI.SetActive(false);
    }

    public void Interact(InteractionData interactionData)
    {
        if (!currentStatus)
        {
            UpdateDoQuest(true);
            questCleaningUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        UpdateDoQuest(false);
    }
}
