using UnityEngine;
using UnityEngine.InputSystem;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private InputActionReference moveInputAction;
    [SerializeField] private float speed = 5f;

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
        Vector2 input = moveInputAction.action.ReadValue<Vector2>() * speed * Time.deltaTime;
        transform.position += new Vector3(input.x, 0, input.y);
    }
}
