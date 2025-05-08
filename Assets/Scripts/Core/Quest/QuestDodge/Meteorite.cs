using UnityEngine;

public class Meteorite : MonoBehaviour
{
    private Rigidbody2D meteoriteRigidbody2D;
    [SerializeField] private float minSpeed = 1000f;
    [SerializeField] private float maxSpeed = 2000f;
    private RectTransform field;
    private RectTransform rectTransform;
    public event System.Action<bool> onHit;
    private float speed;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        meteoriteRigidbody2D = GetComponent<Rigidbody2D>();
        rectTransform = GetComponent<RectTransform>();
        field = transform.parent.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        NewSpawnMeteorite();
    }

    private void FixedUpdate()
    {
        if (IsInField())
        {
            float adjustedSpeed = speed * canvas.scaleFactor;
            meteoriteRigidbody2D.linearVelocity = Vector3.left * adjustedSpeed;
        }
        else
        {
            meteoriteRigidbody2D.linearVelocity = Vector3.left * 0;
            onHit?.Invoke(false);
            NewSpawnMeteorite();
        }
    }

    private bool IsInField()
    {
        Vector2 localPosition = field.InverseTransformPoint(rectTransform.position);

        return field.rect.Contains(localPosition);
    }

    private void NewSpawnMeteorite()
    {
        Vector3[] corners = new Vector3[4];
        field.GetWorldCorners(corners);
        float positionY = Random.Range(corners[3].y + rectTransform.rect.height, corners[2].y - rectTransform.rect.height);
        float positionX = corners[3].x + rectTransform.rect.x;
        transform.position = new Vector3(positionX, positionY, 0);
        speed = Random.Range(minSpeed, maxSpeed);
        rectTransform.localScale = Vector3.one * (1 + MapToRange(speed, minSpeed, maxSpeed));
    }

    float MapToRange(float value, float min, float max)
    {
        value = Mathf.Clamp(value, min, max);

        return (1 - (value - min) / (max - min));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Spaceship"))
        {
            onHit?.Invoke(true);
            NewSpawnMeteorite();
        }
    }
}
