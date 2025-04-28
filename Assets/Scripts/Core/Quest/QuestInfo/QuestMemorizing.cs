using Unity.Netcode;
using UnityEngine;

public class QuestMemorizing : QuestInfo, IInteractable
{
    [SerializeField] private ChangeCamera changeCamera;
    [SerializeField] private QuestMemorizingManager questMemorizingManager;
    [SerializeField] private GameObject questMemorizeUI;

    private void OnEnable()
    {
        questMemorizingManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questMemorizingManager.OnFinishedQuest -= Finished;
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
        if (questMemorizeUI.activeInHierarchy)
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
            changeCamera.SwitchCamera(0, 4);
            UpdateDoQuest(true, interactionData);
            questMemorizeUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        changeCamera.SwitchCamera(4, 0);
        UpdateDoQuest(false, null);
        questMemorizeUI.SetActive(false);
    }
}
