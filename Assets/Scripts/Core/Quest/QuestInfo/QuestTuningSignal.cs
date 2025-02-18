using Unity.Netcode;
using UnityEngine;

public class QuestTuningSignal : QuestInfo, IInteractable
{
    [SerializeField] private QuestTuningManager questTuningManager;
    [SerializeField] private GameObject questTuningUI;

    private void OnEnable()
    {
        questTuningManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questTuningManager.OnFinishedQuest -= Finished;
    }

    private void Finished(bool finished)
    {
        FinishQuest();
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
        if(questTuningUI.activeInHierarchy)
        {
            CancelQuest();
        }
    }

    public void Interact(InteractionData interactionData)
    {
        if (!currentStatus)
        {
            UpdateDoQuest(true, interactionData);
            questTuningUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        UpdateDoQuest(false, null);
        questTuningUI.SetActive(false);
    }
}
