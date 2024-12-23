using Unity.Netcode;

public abstract class QuestInfo : NetworkBehaviour
{
    protected bool QuestStatus = false;
    public event System.Action<bool> OnQuestStatusChanged;

    protected void BrokenQuest()
    {
        if (IsSpawned && QuestStatus)
        {
            UpdateQuestStatusServerRpc(false);
        }
    }

    protected void FinishQuest()
    {
        if (IsSpawned && !QuestStatus)
        {
            UpdateQuestStatusServerRpc(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateQuestStatusServerRpc(bool Status)
    {
        if(!IsHost)
        {
            UpdateQuest(Status);
        }
        UpdateQuestStatusClientRpc(Status);
    }

    [ClientRpc]
    private void UpdateQuestStatusClientRpc(bool Status)
    {
        UpdateQuest(Status);
    }

    private void UpdateQuest(bool Status)
    {
        QuestStatus = Status;
        OnQuestStatusChanged?.Invoke(Status);
    }
}
