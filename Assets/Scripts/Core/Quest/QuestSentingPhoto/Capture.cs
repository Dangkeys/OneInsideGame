using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Capture : MonoBehaviour
{
    [SerializeField] private InputActionReference inputActionReference;
    [SerializeField] private float radius = 100f;
    [SerializeField] private QuestSentingPhotoManager questSentingPhotoManager;

    private void OnEnable()
    {
        inputActionReference.action.performed += OnClick;
        questSentingPhotoManager.OnImageCorrect += OnCorrect;
    }

    private void OnDisable()
    {
        inputActionReference.action.performed -= OnClick;
        questSentingPhotoManager.OnImageCorrect -= OnCorrect;
    }


    private void OnCorrect(bool correct)
    {
        if(correct)
        {
            gameObject.GetComponent<FreeCamera>().DisableMove();
            inputActionReference.action.performed -= OnClick;
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (Physics.SphereCast(transform.position, radius, transform.forward, out RaycastHit hit, 1000f))
        {
            questSentingPhotoManager.CheckPhoto(hit.collider.name);
        }
    }
}
