using Unity.Netcode;
using UnityEngine;

public class QuestDodge : QuestInfo, IInteractable
{
    [SerializeField] private QuestDodgeManager QuestDodgeManager;
    [SerializeField] private GameObject QuestDodgeManagerGameObject;

    private void OnEnable()
    {
        QuestDodgeManager.OnFinishedQuest += HandleWinServerRpc;
    }

    private void OnDisable()
    {
        QuestDodgeManager.OnFinishedQuest -= HandleWinServerRpc;
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleWinServerRpc(bool Win)
    {
        if (!IsHost)
        {
            Winning(Win);
        }
        HandleWinClientRpc(Win);
    }

    [ClientRpc]
    private void HandleWinClientRpc(bool Win)
    {
        Winning(Win);
    }

    private void Winning(bool Win)
    {
        FinishQuest();
        QuestDodgeManagerGameObject.SetActive(false);
    }

    public void Interact(InteractionData interactionData)
    {
        if (!QuestStatus)
        {
            UpdateDoQuest(true);
            QuestDodgeManagerGameObject.SetActive(true);
        }
    }

    public void CancelQuest()
    {
        UpdateDoQuest(false);
    }
}
