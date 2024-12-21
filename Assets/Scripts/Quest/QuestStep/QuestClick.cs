using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestClick : QuestInfo
{
    public InputActionReference InputActionReference;
    private List<string> AllClick = new List<string>();

    private void Start()
    {
        foreach (var binding in InputActionReference.action.bindings)
        {
            string[] pathSegments = binding.path.Split('/');
            string buttonName = pathSegments[pathSegments.Length - 1];
            AllClick.Add(buttonName);
        }
    }

    private void OnEnable()
    {
        InputActionReference.action.started += HandleQuestClick;
    }

    private void OnDisable()
    {
        InputActionReference.action.started -= HandleQuestClick;
    }

    private void HandleQuestClick(InputAction.CallbackContext context)
    {
        Debug.Log(context.control.displayName);
    }
}
