using UnityEngine;

public class CrossCubeController : MonoBehaviour
{
    public bool Lower { private set; get; } = true;
    public System.Action<bool> OnReached;
    private Vector3[] position = new Vector3[2];
    [SerializeField] private float distance;

    private void Awake()
    {
        position[0] = transform.position;
        position[1] = transform.position + Vector3.forward * distance;
    }

    private void OnDisable()
    {
        Lower = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Start"))
        {
            if (!Lower)
            {
                Lower = true;
                OnReached?.Invoke(true);
            }
        }
        else if (collision.collider.CompareTag("Goal"))
        {
            if(Lower)
            {
                Lower = false;
                OnReached?.Invoke(true);
            }
        }
        else if(collision.collider.CompareTag("Sky"))
        {
            OnReached?.Invoke(false);
            if(Lower)
            {
                transform.position = position[0];
            }
            else
            {
                transform.position = position[1];
            }
        }
    }
}
