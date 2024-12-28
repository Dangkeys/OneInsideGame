using Unity.Netcode;
using UnityEngine;

public abstract class QuestInfo : NetworkBehaviour
{
    protected bool QuestStatus = false;
    public event System.Action<bool> OnQuestStatusChanged;
    public event System.Action<bool> OnDoQuest;

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
            UpdateDoQuest(false);
            UpdateQuestStatusServerRpc(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateQuestStatusServerRpc(bool Status)
    {
        OnQuestStatusChanged?.Invoke(Status);
        if (!IsHost)
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
    }

    protected void UpdateDoQuest(bool DoQuest)
    {
        OnDoQuest?.Invoke(DoQuest);
        Debug.Log(NetworkManager.Singleton.LocalClientId);
    }
}
