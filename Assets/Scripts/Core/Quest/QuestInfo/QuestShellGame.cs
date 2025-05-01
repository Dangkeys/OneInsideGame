using Unity.Netcode;
using UnityEngine;

public class QuestShellGame : QuestInfo, IInteractable
{
    [SerializeField] private ChangeCamera changeCamera;
    [SerializeField] private QuestShellGameManager questShellGameManager;
    [SerializeField] private GameObject questShellGameUI;

    private void OnEnable()
    {
        questShellGameManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        questShellGameManager.OnFinishedQuest -= Finished;
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
        if (questShellGameUI.activeInHierarchy)
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
            changeCamera.SwitchCamera(0, 6);
            UpdateDoQuest(true, interactionData);
            questShellGameUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        changeCamera.SwitchCamera(6, 0);
        UpdateDoQuest(false, null);
        questShellGameUI.SetActive(false);
    }
}
