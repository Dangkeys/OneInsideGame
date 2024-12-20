using System;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public float InteractRange { get; private set; }

    void Start()
    {
        InputReader.InteractEvent += TryToInteract;
    }

    private void TryToInteract()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, InteractRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
            }
        }
    }

    // Draw debug visualization
    private void OnDrawGizmos()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, InteractRange);
        
        // Draw raycast direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * InteractRange);
    }

    private void OnDestroy() 
    {
        InputReader.InteractEvent -= TryToInteract;
    }
}