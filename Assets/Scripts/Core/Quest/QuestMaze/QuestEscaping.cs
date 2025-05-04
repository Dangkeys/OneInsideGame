using Unity.Netcode;
using UnityEngine;

public class QuestEscaping : QuestInfo, IInteractable
{
    [SerializeField] private ChangeCamera changeCamera;
    [SerializeField] private QuestEscapingManager questEscapingManager;
    [SerializeField] private GameObject questEscapingUI;

    private void OnEnable()
    {
        questEscapingManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questEscapingManager.OnFinishedQuest -= Finished;
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
        if (questEscapingUI.activeInHierarchy)
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
            changeCamera.SwitchCamera(0, 5);
            UpdateDoQuest(true, interactionData);
            questEscapingUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        changeCamera.SwitchCamera(5, 0);
        UpdateDoQuest(false, null);
        questEscapingUI.SetActive(false);
    }
}
