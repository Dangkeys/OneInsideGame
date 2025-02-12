using UnityEngine;
using UnityEngine.InputSystem;

public class Wave : MonoBehaviour
{
    [SerializeField] private InputActionReference inputActionReference;
    private Rigidbody2D waveRigidbody2D;
    [SerializeField] private float speed = 1000f;
    private Vector2 movement = Vector2.right;
    [SerializeField] private RectTransform field;
    private RectTransform rectTransform;
    public event System.Action<bool> OnEndDirection;
    private Vector3 initPosition;

    private void Awake()
    {
        waveRigidbody2D = GetComponent<Rigidbody2D>();
        rectTransform = GetComponent<RectTransform>();
        initPosition = transform.position;
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
        GoToStart();
    }

    private void FixedUpdate()
    {
        Vector2 desiredPosition = waveRigidbody2D.position + movement * speed * Time.fixedDeltaTime;

        Vector2 clampedPosition = ClampPosition(desiredPosition);

        waveRigidbody2D.linearVelocity = (clampedPosition - waveRigidbody2D.position) / Time.fixedDeltaTime;

        if(waveRigidbody2D.linearVelocity.x <= 0)
        {
            OnEndDirection?.Invoke(true);
            GoToStart();
        }
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        movement = new Vector2(1, context.ReadValue<Vector2>().y); 
    }

    private void StopMove(InputAction.CallbackContext context)
    {
        movement = new Vector2(1, 0);
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

    private void GoToStart()
    {
        Vector3 position = new Vector3(initPosition.x, transform.position.y);
        transform.position = position;
    }
}
