using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputControls;

public class InputReader : Singleton<InputReader>, IPlayerActions
{
    private InputControls controls;
    public Vector2 MovementValue { get; private set; }
    public Action InteractEvent;
    public Action AttackEvent;
    public Action TranformationEvent;

    public Action<bool> UseEvent;
    public Action<bool> SprintEvent;

    public Action<bool> JumpEvent;

    public Action OpenSabotageUIEvent;

    public Action UseAbilityEvent;
    public static Action<int> Hold1stItem;
    public static Action<int> Hold2ndItem;
    public static Action<int> Hold3rdItem;
    public static Action<int> Hold4thItem;
    public static Action<int> Hold5thItem;
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

    public void OnTransformation(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            TranformationEvent?.Invoke();
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




    public void OnNext(InputAction.CallbackContext context)
    {
        //OPTIONAL
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        //OPTIONAL
    }

    public void OnOpenSabotageWindow(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OpenSabotageUIEvent?.Invoke();
        }
    }

    public void OnUseAbility(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UseAbilityEvent?.Invoke();
        }
    }

    public void OnHold1stItem(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Hold1stItem?.Invoke(1);
        }
    }

    public void OnHold2ndItem(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Hold2ndItem?.Invoke(2);
        }
    }

    public void OnHold3rdItem(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Hold3rdItem?.Invoke(3);
        }
    }

    public void OnHold4thItem(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Hold4thItem?.Invoke(4);
        }
    }
    
    public void OnHold5thItem(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Hold5thItem?.Invoke(5);
        }
    }
}
