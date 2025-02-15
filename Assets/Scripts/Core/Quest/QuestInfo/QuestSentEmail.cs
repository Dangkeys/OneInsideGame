using Unity.Netcode;
using UnityEngine;

public class QuestSentEmail : QuestInfo, IInteractable
{
    [SerializeField] private ChangeCamera changeCamera;
    [SerializeField] private QuestSentingEmailManager questSentingEmailManager;
    [SerializeField] private GameObject questSentingUI;

    private void OnEnable()
    {
        questSentingEmailManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questSentingEmailManager.OnFinishedQuest -= Finished;
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
        if (questSentingUI.activeInHierarchy)
        {
            CancelQuest();
        }
    }

    public void Interact(InteractionData interactionData)
    {
        if (!currentStatus)
        {
            changeCamera.SwitchCamera(0, 2);
            UpdateDoQuest(true, interactionData);
            questSentingUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        changeCamera.SwitchCamera(2, 0);
        UpdateDoQuest(false, null);
        questSentingUI.SetActive(false);
    }
}
