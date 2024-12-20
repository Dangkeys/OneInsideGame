using Unity.Netcode;
using UnityEngine;

public abstract class QuestInfo : NetworkBehaviour
{
    private bool QuestStatus = false;
    public event System.Action<bool> OnQuestStatusChanged;

    protected void BrokenQuest()
    {
        if (QuestStatus)
        {
            UpdateQuestStatusServerRpc(false);
        }
    }

    protected void FinishQuest()
    {
        if (!QuestStatus)
        {
            UpdateQuestStatusServerRpc(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateQuestStatusServerRpc(bool Status)
    {
        UpdateQuestStatusClientRpc(Status);
    }

    [ClientRpc]
    private void UpdateQuestStatusClientRpc(bool Status)
    {
        QuestStatus = Status;
        OnQuestStatusChanged?.Invoke(Status);
    }
}
