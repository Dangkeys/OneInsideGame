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
        inputActionReference.action.started += HandleClick;
        questClickManager.onChangedWord += HandleWord;
        questClickManager.OnFinishedQuest += HandleFinishedServerRpc;
    }

    private void OnDisable()
    {
        inputActionReference.action.started -= HandleClick;
        questClickManager.onChangedWord -= HandleWord;
        questClickManager.OnFinishedQuest -= HandleFinishedServerRpc;
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleFinishedServerRpc(bool finished)
    {
        if (!IsHost)
        {
            Finished(finished);
        }
        HandleFinishedClientRpc(finished);
    }

    [ClientRpc]
    private void HandleFinishedClientRpc(bool finished)
    {
        Finished(finished);
    }

    private void Finished(bool finished)
    {
        FinishQuest();
        questClickUI.SetActive(false);
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
        if(!currentStatus)
        {
            UpdateDoQuest(true);
            questClickUI.SetActive(true);
        }
    }

    public override void CancelQuest()
    {
        UpdateDoQuest(false);
    }
}
