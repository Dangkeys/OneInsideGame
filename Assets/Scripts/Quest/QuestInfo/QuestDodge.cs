using Unity.Netcode;
using UnityEngine;

public class QuestDodge : QuestInfo, IInteractable
{
    [SerializeField] private QuestDodgeManager questDodgeManager;
    [SerializeField] private GameObject questDodgeUI;

    private void OnEnable()
    {
        questDodgeManager.OnFinishedQuest += HandleFinishedServerRpc;
    }

    private void OnDisable()
    {
        questDodgeManager.OnFinishedQuest -= HandleFinishedServerRpc;
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleFinishedServerRpc(bool finished)
    {
        if (!IsHost)
        {
            Finished(finished);
        }
        HandleFinishedClientRpc(finished);
    }

    [ClientRpc]
    private void HandleFinishedClientRpc(bool finished)
    {
        Finished(finished);
    }

    private void Finished(bool finished)
    {
        FinishQuest();
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
