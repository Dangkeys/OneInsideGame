using Unity.Netcode;
using UnityEngine;

public class QuestCrossCollect : QuestInfo, IInteractable
{
    [SerializeField] private ChangeCamera changeCamera;
    [SerializeField] private QuestCrossCollectManager questCrossCollectManager;
    [SerializeField] private GameObject questCrossCollectUI;

    private void OnEnable()
    {
        questCrossCollectManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questCrossCollectManager.OnFinishedQuest -= Finished;
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
        if (questCrossCollectUI.activeInHierarchy)
        {
            CancelQuest();
        }
    }

    public void Interact(InteractionData interactionData)
    {
        if (index < 0)
            return;
        if (!currentStatus)
        {
            changeCamera.SwitchCamera(0, 8);
            UpdateDoQuest(true, interactionData);
            questCrossCollectUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        changeCamera.SwitchCamera(8, 0);
        UpdateDoQuest(false, null);
        questCrossCollectUI.SetActive(false);
    }

}
