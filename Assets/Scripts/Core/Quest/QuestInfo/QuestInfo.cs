using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class QuestInfo : NetworkBehaviour
{
    protected bool currentStatus = false;
    public event System.Action<int, bool> questInfoStatus;
    private PlayerMovement playerMovement;
    protected int index = -1;

    public void Init(int newIndex)
    {
        StartCoroutine(WaitForSpawnAndUpdate());
        UpdateQuestIndexServerRpc(newIndex);
    }

    private IEnumerator WaitForSpawnAndUpdate()
    {
        yield return new WaitUntil(() => IsSpawned);
        UpdateQuestLayoutServerRpc(false);
    }

    protected void BreakQuest()
    {
        if (index < 0)
            return;
        if (IsSpawned && currentStatus)
        {
            UpdateQuestStatusServerRpc(false);
            UpdateQuestLayoutServerRpc(false);
        }
    }

    protected void FinishQuest()
    {
        if (index < 0)
            return;
        if (IsSpawned && !currentStatus)
        {
            UpdateQuestStatusServerRpc(true);
            UpdateQuestLayoutServerRpc(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateQuestStatusServerRpc(bool status)
    {
        if (index < 0)
            return;
        questInfoStatus?.Invoke(index, status);
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

    [ServerRpc(RequireOwnership = false)]
    private void UpdateQuestLayoutServerRpc(bool status)
    {
        if (!IsHost)
        {
            UpdateQuestLayout(status);
        }
        UpdateQuestLayoutClientRpc(status);
    }

    [ClientRpc]
    private void UpdateQuestLayoutClientRpc(bool status)
    {
        UpdateQuestLayout(status);
    }

    private void UpdateQuestLayout(bool status)
    {
        if (status)
        {
            gameObject.layer = LayerMask.NameToLayer("QuestEnd");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("QuestStart");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateQuestIndexServerRpc(int index)
    {
        if (!IsHost)
        {
            UpdateQuestIndex(index);
        }
        UpdateQuestIndexClientRpc(index);
    }

    [ClientRpc]
    private void UpdateQuestIndexClientRpc(int index)
    {
        UpdateQuestIndex(index);
    }

    private void UpdateQuestIndex(int newIndex)
    {
        index = newIndex;
    }

    protected void UpdateDoQuest(bool doQuest, InteractionData interactionData)
    {
        if (index < 0)
            return;
        if (!playerMovement && interactionData != null)
        {
            playerMovement = interactionData.Interactor.gameObject.GetComponent<PlayerMovement>();
        }
        if (playerMovement)
        {
            playerMovement.EnablePlayerMovement(!doQuest);
        }
    }

    public abstract void CancelQuest();
}
