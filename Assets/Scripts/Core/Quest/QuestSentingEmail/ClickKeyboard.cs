using UnityEngine;
using UnityEngine.InputSystem;

public class ClickKeyboard : MonoBehaviour
{
    [SerializeField] private InputActionReference inputActionReference;
    [SerializeField] private QuestSentingEmailManager questSentingEmailManager;
    private bool capsLock = false;
    private bool shift = false;

    private void OnEnable()
    {
        inputActionReference.action.performed += OnClick;
    }

    private void OnDisable()
    {
        inputActionReference.action.performed -= OnClick;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit, 100f))
        {
            ProcessInput(hit.collider.name);
        }
    }

    private void ProcessInput(string input)
    {
        if (input.Length == 1)
        {
            HandleCharacterInput(input);
        }
        else
        {
            HandleSpecialKeys(input);
        }
    }

    private void HandleCharacterInput(string input)
    {
        string processedInput = capsLock ^ shift ? input.ToUpper() : input.ToLower();
        shift = false;
        SendData(processedInput);
    }

    private void HandleSpecialKeys(string input)
    {
        switch (input)
        {
            case "Tab":
                SendData("   ");
                break;
            case "Caps Lock":
                capsLock = !capsLock;
                break;
            case "Shift":
                shift = true;
                break;
            case "Delete":
                Delete();
                break;
            case "Enter":
                Confirm();
                break;
            case "Spacebar":
                SendData(" ");
                break;
            default:
                break;
        }
    }

    private void SendData(string data)
    {
        questSentingEmailManager.AddWritingWord(data);
    }

    private void Delete()
    {
        questSentingEmailManager.DeleteWritingWord();
    }

    private void Confirm()
    {
        questSentingEmailManager.CheckWord();
    }

}
