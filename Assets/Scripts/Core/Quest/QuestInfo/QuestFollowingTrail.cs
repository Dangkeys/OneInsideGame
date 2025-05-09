using Unity.Netcode;
using UnityEngine;

public class QuestFollowingTrail : QuestInfo, IInteractable
{
    [SerializeField] private ChangeCamera changeCamera;
    [SerializeField] private QuestFollowingTrailManager questFollowingTrailManager;
    [SerializeField] private GameObject questFollowingTrailUI;

    private void OnEnable()
    {
        questFollowingTrailManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questFollowingTrailManager.OnFinishedQuest -= Finished;
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
        if (questFollowingTrailUI.activeInHierarchy)
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
            changeCamera.SwitchCamera(0, 7);
            UpdateDoQuest(true, interactionData);
            questFollowingTrailUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        changeCamera.SwitchCamera(7, 0);
        UpdateDoQuest(false, null);
        questFollowingTrailUI.SetActive(false);
    }
}
