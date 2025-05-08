using Unity.Netcode;
using UnityEngine;

public class QuestSentingPhoto : QuestInfo, IInteractable
{
    [SerializeField] private ChangeCamera changeCamera;
    [SerializeField] private QuestSentingPhotoManager questSentingPhotoManager;
    [SerializeField] private GameObject questSentingUI;

    private void OnEnable()
    {
        questSentingPhotoManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questSentingPhotoManager.OnFinishedQuest -= Finished;
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
        if (index < 0)
            return;
        if (!currentStatus)
        {
            changeCamera.SwitchCamera(0, 3);
            UpdateDoQuest(true, interactionData);
            questSentingUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        changeCamera.SwitchCamera(3, 0);
        UpdateDoQuest(false, null);
        questSentingUI.SetActive(false);
    }
}
