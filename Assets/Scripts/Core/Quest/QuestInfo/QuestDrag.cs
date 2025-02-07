using Unity.Netcode;
using UnityEngine;

public class QuestDrag : QuestInfo, IInteractable
{
    [SerializeField] private QuestDragManager questDragManager;
    [SerializeField] private GameObject questDragUI;

    private void OnEnable()
    {
        questDragManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questDragManager.OnFinishedQuest -= Finished;
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
        questDragUI.SetActive(false);
    }

    public void Interact(InteractionData interactionData)
    {
        if (!currentStatus)
        {
            UpdateDoQuest(true);
            questDragUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        UpdateDoQuest(false);
    }
}
