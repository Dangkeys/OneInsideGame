using Unity.Netcode;
using UnityEngine;

public class QuestShootingDebris : QuestInfo, IInteractable
{
    [SerializeField] private ChangeCamera changeCamera;
    [SerializeField] private GameObject questShootingUI;
    [SerializeField] private QuestShootingManager questShootingManager;

    private void OnEnable()
    {
        questShootingManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questShootingManager.OnFinishedQuest -= Finished;
    }

    private void Finished(bool finished)
    {
        if (finished)
        {
            FinishQuest();
            HandleFinishedServerRpc(finished);
        }
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
        if(questShootingUI.activeInHierarchy)
        {
            CancelQuest();
        }
    }

    public void Interact(InteractionData interactionData)
    {
        if (!currentStatus)
        {
            changeCamera.SwitchCamera(0, 1);
            UpdateDoQuest(true, interactionData);
            questShootingUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        UpdateDoQuest(false, null);
        changeCamera.SwitchCamera(1, 0);
        questShootingUI.SetActive(false);
    }
}
