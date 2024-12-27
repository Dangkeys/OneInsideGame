using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles interaction with nearby interactable objects in the game world
/// </summary>
public class Interactor : MonoBehaviour
{
    [field: SerializeField, Tooltip("Reference to the input system")]
    public InputReader InputReader { get; private set; }

    [field: SerializeField, Tooltip("Maximum distance at which interactions can occur")] 
    public float InteractionRadius { get; private set; }

    private void Start()
    {
        InputReader.InteractEvent += HandleInteractionAttempt;
    }

    private void HandleInteractionAttempt()
    {
        IInteractable nearestInteractable = FindNearestInteractable();
        if (nearestInteractable != null)
        {
            nearestInteractable.Interact();
        }
    }

    private IInteractable FindNearestInteractable()
    {
        List<IInteractable> nearbyInteractables = FindInteractablesInRadius();
        if (nearbyInteractables.Count == 0) return null;

        return FindClosestFrom(nearbyInteractables);
    }

    private List<IInteractable> FindInteractablesInRadius()
    {
        List<IInteractable> interactables = new List<IInteractable>();
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, InteractionRadius);

        foreach (Collider collider in nearbyColliders)
        {
            if (collider.TryGetComponent(out IInteractable interactable))
            {
                interactables.Add(interactable);
            }
        }
        return interactables;
    }

    private IInteractable FindClosestFrom(List<IInteractable> interactables)
    {
        IInteractable closest = null;
        float shortestDistance = float.MaxValue;

        foreach (IInteractable interactable in interactables)
        {
            Vector3 interactablePosition = ((MonoBehaviour)interactable).transform.position;
            float distance = Vector3.Distance(transform.position, interactablePosition);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }

    private void OnDrawGizmos()
    {
        DrawInteractionRadius();
        DrawInteractionDirection();
    }

    private void DrawInteractionRadius()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, InteractionRadius);
    }

    private void DrawInteractionDirection()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * InteractionRadius);
    }

    private void OnDestroy()
    {
        InputReader.InteractEvent -= HandleInteractionAttempt;
    }
}