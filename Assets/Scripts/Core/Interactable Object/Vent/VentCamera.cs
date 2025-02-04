using UnityEngine;
using UnityEngine.InputSystem;
public class VentCamera : MonoBehaviour
{
    public float LookSpeed = 1f;

    private Vector2 lookInput;
    private float yaw, pitch;

    void Start()
    {
        yaw = transform.localEulerAngles.y;
        pitch = transform.localEulerAngles.x;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>() * LookSpeed;
    }

    private void Update()
    {
        yaw += lookInput.x * LookSpeed;
        pitch -= lookInput.y * LookSpeed;
        pitch = Mathf.Clamp(pitch, 0f, 45f);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
