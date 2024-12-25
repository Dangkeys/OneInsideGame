using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [SerializeField] private InputActionReference InputActionReference;
    private Rigidbody2D Rigidbody2D;
    [SerializeField] private float Speed = 1000f;
    private Vector2 Movement = Vector2.zero;
    private RectTransform Field;
    private RectTransform MyRectTransform;

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        MyRectTransform = GetComponent<RectTransform>();
        Field = transform.parent.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        InputActionReference.action.performed += HandleMove;
        InputActionReference.action.canceled += StopMove;
    }

    private void OnDisable()
    {
        InputActionReference.action.performed -= HandleMove;
        InputActionReference.action.canceled -= StopMove;
    }

    private void FixedUpdate()
    {
        Vector2 DesiredPosition = Rigidbody2D.position + Movement * Speed * Time.fixedDeltaTime;

        Vector2 ClampedPosition = ClampPosition(DesiredPosition);

        Rigidbody2D.linearVelocity = (ClampedPosition - Rigidbody2D.position) / Time.fixedDeltaTime;
    }

    private void HandleMove(InputAction.CallbackContext Context)
    {
        Movement = Context.ReadValue<Vector2>();
    }

    private void StopMove(InputAction.CallbackContext Context)
    {
        Movement = Vector2.zero;
    }

    private Vector2 ClampPosition(Vector2 DesiredPosition)
    {
        Vector3[] Corners = new Vector3[4];
        Field.GetWorldCorners(Corners);

        float MinX = Corners[0].x + MyRectTransform.rect.width / 2;
        float MaxX = Corners[2].x - MyRectTransform.rect.width / 2;
        float MinY = Corners[0].y + MyRectTransform.rect.height / 2;
        float MaxY = Corners[2].y - MyRectTransform.rect.height / 2;

        DesiredPosition.x = Mathf.Clamp(DesiredPosition.x, MinX, MaxX);
        DesiredPosition.y = Mathf.Clamp(DesiredPosition.y, MinY, MaxY);

        return DesiredPosition;
    }
}
