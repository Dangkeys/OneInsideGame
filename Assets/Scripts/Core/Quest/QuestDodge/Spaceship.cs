using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [SerializeField] private InputActionReference inputActionReference;
    private Rigidbody2D spaceshipRigidbody2D;
    [SerializeField] private float speed = 1000f;
    private Vector2 movement = Vector2.zero;
    private RectTransform field;
    private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        spaceshipRigidbody2D = GetComponent<Rigidbody2D>();
        rectTransform = GetComponent<RectTransform>();
        field = transform.parent.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        inputActionReference.action.performed += HandleMove;
        inputActionReference.action.canceled += StopMove;
    }

    private void OnDisable()
    {
        inputActionReference.action.performed -= HandleMove;
        inputActionReference.action.canceled -= StopMove;
    }

    private void FixedUpdate()
    {
        Vector2 desiredPosition = spaceshipRigidbody2D.position + movement * speed * canvas.scaleFactor * Time.fixedDeltaTime;

        Vector2 clampedPosition = ClampPosition(desiredPosition);

        spaceshipRigidbody2D.linearVelocity = (clampedPosition - spaceshipRigidbody2D.position) / Time.fixedDeltaTime;
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    private void StopMove(InputAction.CallbackContext context)
    {
        movement = Vector2.zero;
    }

    private Vector2 ClampPosition(Vector2 desiredPosition)
    {
        Vector3[] corners = new Vector3[4];
        field.GetWorldCorners(corners);

        float minX = corners[0].x + rectTransform.rect.width / 2;
        float maxX = corners[2].x - rectTransform.rect.width / 2;
        float minY = corners[0].y + rectTransform.rect.height / 2;
        float maxY = corners[2].y - rectTransform.rect.height / 2;

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        return desiredPosition;
    }
}
