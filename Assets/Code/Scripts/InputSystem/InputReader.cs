using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputControls;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<Vector2> MoveEvent;

    private InputControls controls;


    private void OnEnable()
    {

        Debug.Log("InputReader enabled");
        Initialize();
    }

    public void Initialize()
    {
        if (controls == null)
        {
            controls = new InputControls();
            controls.Player.SetCallbacks(this);
        }

        EnableGameplayInput();
    }

    public void EnableGameplayInput()
    {
        controls?.Player.Enable();
    }

    public void DisableGameplayInput()
    {
        controls?.Player.Disable();
    }

    private void OnDisable()
    {
        Debug.Log("InputReader disabled");
        DisableGameplayInput();
    }

    private void OnDestroy()
    {
        controls?.Dispose();
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<Vector2>());
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }
}
