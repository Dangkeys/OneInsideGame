using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private Rigidbody rb;
    public Vector3 Direction { set; private get; }
    [SerializeField] private float speed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = Direction * speed;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Floor"))
        {
            gameObject.SetActive(false);
        }
    }
}
