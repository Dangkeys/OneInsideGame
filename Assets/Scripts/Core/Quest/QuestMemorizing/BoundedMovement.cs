using UnityEngine;
using UnityEngine.InputSystem;

public class BoundedMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Rigidbody rb;
    [SerializeField] private GameObject area;
    [SerializeField] private InputActionReference inputActionReference;

    private Vector3 minBounds;
    private Vector3 maxBounds;

    private Vector3 initPosition;
    private Vector2 moveInput = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        initPosition = transform.position;

        Collider areaCollider = area.GetComponent<Collider>();
        if (areaCollider != null)
        {
            minBounds = areaCollider.bounds.min;
            maxBounds = areaCollider.bounds.max;
        }
        else
        {
            Debug.LogError("Area GameObject needs a Collider!");
        }
    }

    private void OnEnable()
    {
        transform.position = initPosition;
        inputActionReference.action.Enable();
        inputActionReference.action.performed += HandleMove;
        inputActionReference.action.canceled += StopMove;
    }

    private void OnDisable()
    {
        inputActionReference.action.Disable();
        inputActionReference.action.performed -= HandleMove;
        inputActionReference.action.canceled -= StopMove;
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void StopMove(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * speed;

        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        Vector3 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minBounds.z, maxBounds.z);

        rb.position = clampedPosition;
    }
}
