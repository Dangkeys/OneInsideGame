using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputControls;

public class InputReader : MonoBehaviour, IPlayerActions, ISettingsActions, IUIActions
{
    private InputControls controls;
    public Vector2 MovementValue { get; private set; }
    public Action InteractEvent;
    public Action AttackEvent;
    public Action<bool> UseEvent;
    public Action<bool> SprintEvent;

    public Action<bool> JumpEvent;

    public Action ActivateAbilityEvent;

    public Action EscapeEvent;

    private void Start()
    {
        Initialize();
    }


    private void Initialize()
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
    public void OnMove(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            AttackEvent?.Invoke();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InteractEvent?.Invoke();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            SprintEvent?.Invoke(false);
        }

    }

    public void OnUse(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UseEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            UseEvent?.Invoke(false);
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            JumpEvent?.Invoke(false);
        }
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        //ALREADY USE IN CINEMACHINE
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        //OPTIONAL
    }
    public void OnEscape(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            EscapeEvent?.Invoke();
        }
    }
    public void OnActivateAbility(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ActivateAbilityEvent?.Invoke();
        }
    }



    public void OnNext(InputAction.CallbackContext context)
    {
        //OPTIONAL
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        //OPTIONAL
    }


    #region UI
    public void OnNavigate(InputAction.CallbackContext context)
    {
        //optional
    }
    public void OnSubmit(InputAction.CallbackContext context)
    {
        //optional
    }
    public void OnCancel(InputAction.CallbackContext context)
    {
        //optional
    }
    public void OnPoint(InputAction.CallbackContext context)
    {
        //optional
    }
    public void OnClick(InputAction.CallbackContext context)
    {
        //optional
    }
    public void OnRightClick(InputAction.CallbackContext context)
    {
        //optional
    }
    public void OnMiddleClick(InputAction.CallbackContext context)
    {
        //optional
    }
    public void OnScrollWheel(InputAction.CallbackContext context)
    {
        //optional
    }
    public void OnTrackedDevicePosition(InputAction.CallbackContext context)
    {
        //optional
    }
    public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
    {
        //optional
    }
    #endregion

}
