using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestClick : QuestInfo, IInteractable
{
    [SerializeField] private InputActionReference InputActionReference;
    [SerializeField] private CircleClick CircleClick;
    [SerializeField] private GameObject CircleClickGameObject;
    private string Word;
    private void Awake()
    {
        foreach (var binding in InputActionReference.action.bindings)
        {
            string[] pathSegments = binding.path.Split('/');
            string buttonName = pathSegments[pathSegments.Length - 1];
            CircleClick.AddWordList(buttonName);
        }
    }

    private void OnEnable()
    {
        InputActionReference.action.started += HandleQuestClick;
        CircleClick.OnWordChange += HandleWordChange;
        CircleClick.OnWin += HandleWinServerRpc;
    }

    private void OnDisable()
    {
        InputActionReference.action.started -= HandleQuestClick;
        CircleClick.OnWordChange -= HandleWordChange;
        CircleClick.OnWin -= HandleWinServerRpc;
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleWinServerRpc(bool Win)
    {
        if (!IsHost)
        {
            Winning(Win);
        }
        HandleWinClientRpc(Win);
    }

    [ClientRpc]
    private void HandleWinClientRpc(bool Win)
    {
        Winning(Win);
    }

    private void Winning(bool Win)
    {
        FinishQuest();
        CircleClickGameObject.SetActive(false);
    }
    private void HandleWordChange(string NewWord)
    {
        Word = NewWord;
    }

    private void HandleQuestClick(InputAction.CallbackContext context)
    {
        if(CircleClickGameObject.activeInHierarchy && Word.ToLower() == context.control.displayName.ToLower())
        {
            CircleClick.UpdateScore(true);
        }
    }

    public void Interact()
    {
        if(!QuestStatus)
        {
            UpdateDoQuest(true);
            CircleClickGameObject.SetActive(true);
        }
    }

    
}
