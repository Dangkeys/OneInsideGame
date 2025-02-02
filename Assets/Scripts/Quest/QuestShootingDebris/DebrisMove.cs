using UnityEngine;

public class DebrisMove : MonoBehaviour
{
    private Vector3 movement;
    private Rigidbody rb;
    [SerializeField] private float speed = 5f;
    private Vector3 finishPosition;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private QuestShootingManager questShootingManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = movement * speed * Time.deltaTime;
        if (Vector3.Distance(finishPosition, transform.position) < 0.1f)
        {
            questShootingManager.TakeDamage();
            gameObject.SetActive(false);
        }
    }

    public void Setinit(Vector3 newMovement, Vector3 location, Vector3 lastLocation)
    {
        transform.position = location;
        movement = newMovement;
        finishPosition = lastLocation;
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
}
