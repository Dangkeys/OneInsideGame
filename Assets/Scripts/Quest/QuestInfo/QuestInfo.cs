using Unity.Netcode;
using UnityEngine;

public abstract class QuestInfo : NetworkBehaviour
{
    protected bool currentStatus = false;
    public event System.Action<bool> questInfoStatus;
    public event System.Action<bool> onDoQuest;

    protected void BreakQuest()
    {
        if (IsSpawned && currentStatus)
        {
            UpdateQuestStatusServerRpc(false);
        }
    }

    protected void FinishQuest()
    {
        if (IsSpawned && !currentStatus)
        {
            UpdateDoQuest(false);
            UpdateQuestStatusServerRpc(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateQuestStatusServerRpc(bool status)
    {
        questInfoStatus?.Invoke(status);
        if (!IsHost)
        {
            UpdateQuestStatus(status);
        }
        UpdateQuestStatusClientRpc(status);
    }

    [ClientRpc]
    private void UpdateQuestStatusClientRpc(bool status)
    {
        UpdateQuestStatus(status);
    }

    private void UpdateQuestStatus(bool status)
    {
        currentStatus = status;
    }

    protected void UpdateDoQuest(bool doQuest)
    {
        onDoQuest?.Invoke(doQuest);
        Debug.Log(NetworkManager.Singleton.LocalClientId);
    }

    public abstract void CancelQuest();
}
