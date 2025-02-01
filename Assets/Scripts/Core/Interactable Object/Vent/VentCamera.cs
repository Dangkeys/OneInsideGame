using UnityEngine;

public class VentCamera : MonoBehaviour
{
    public float Sensitivity = 2f;
    float rotationX = 0f;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * Sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Sensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, 0f, 45f);

        transform.localRotation = Quaternion.Euler(rotationX, transform.localRotation.eulerAngles.y + mouseX, 0);
    }
}
