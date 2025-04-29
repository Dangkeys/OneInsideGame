using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;    
    [SerializeField] private Vector3 offset;      
    [SerializeField] private float smoothSpeed = 0.125f;
    private Quaternion rotation;

    private void Start()
    {
        rotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            transform.position = smoothedPosition;

            transform.rotation = rotation;
        }
    }
}

