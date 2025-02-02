using UnityEngine;

public class BulletMove : MonoBehaviour
{
    private Vector3 movement;
    private Rigidbody rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 10f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = movement * speed * Time.deltaTime;
    }

    public void Setinit(Vector3 newMovement, Vector3 location)
    {
        transform.position = location;
        movement = newMovement;
    }

    private void OnEnable()
    {
        CancelInvoke(); 
        Invoke(nameof(SetInactive), lifetime);
    }

    private void SetInactive()
    {
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Debris"))
        {
            collision.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
