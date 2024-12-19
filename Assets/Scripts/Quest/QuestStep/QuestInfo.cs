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
    private void UpdateQuestStatusServerRpc(bool status)
    {
        Debug.Log($"Server: Quest status changed to {status}");
        UpdateQuestStatusClientRpc(status);
    }

    [ClientRpc]
    private void UpdateQuestStatusClientRpc(bool status)
    {
        Debug.Log($"Client: Quest status changed to {status}");
        QuestStatus = status;
        OnQuestStatusChanged?.Invoke(status);
    }
}
