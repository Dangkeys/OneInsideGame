using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCamera : MonoBehaviour
{
    [SerializeField] private InputActionReference moveInputAction;
    [SerializeField] private float sensitivity = 5f;
    private float rotationX = 0f;
    private float rotationY = 0f;

    private void OnEnable()
    {
        moveInputAction.action.Enable();
    }

    private void OnDisable()
    {
        moveInputAction.action.Disable();
    }

    private void Update()
    {
        Vector2 input = moveInputAction.action.ReadValue<Vector2>() * sensitivity * Time.deltaTime;

        rotationX -= input.y;
        rotationY += input.x;

        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    public void DisableMove()
    {
        moveInputAction.action.Disable();
    }
}
