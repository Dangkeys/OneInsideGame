using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestClick : QuestInfo, IInteractable
{
    [SerializeField] private InputActionReference inputActionReference;
    [SerializeField] private QuestClickManager questClickManager;
    [SerializeField] private GameObject questClickUI;
    private string word;
    private void Awake()
    {
        foreach (var binding in inputActionReference.action.bindings)
        {
            string[] pathSegments = binding.path.Split('/');
            string buttonName = pathSegments[pathSegments.Length - 1];
            questClickManager.AddWordList(buttonName);
        }
    }

    private void OnEnable()
    {
        inputActionReference.action.Enable();
        inputActionReference.action.started += HandleClick;
        questClickManager.onChangedWord += HandleWord;
        questClickManager.OnFinishedQuest += Finished;
    }

    private void OnDisable()
    {
        inputActionReference.action.Disable();
        inputActionReference.action.started -= HandleClick;
        questClickManager.onChangedWord -= HandleWord;
        questClickManager.OnFinishedQuest -= Finished;
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
        if(questClickUI.activeInHierarchy)
        {
            CancelQuest();
        }
    }
    private void HandleWord(string newWord)
    {
        word = newWord;
    }

    private void HandleClick(InputAction.CallbackContext context)
    {
        if(questClickUI.activeInHierarchy && word.ToLower() == context.control.displayName.ToLower())
        {
            questClickManager.UpdateScore(true);
        }
    }

    public void Interact(InteractionData interactionData)
    {
        if (index < 0)
            return;
        if (!currentStatus)
        {
            UpdateDoQuest(true, interactionData);
            questClickUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        UpdateDoQuest(false, null);
        questClickUI.SetActive(false);
    }
}
