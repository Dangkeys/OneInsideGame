using UnityEngine;

public class Meteorite : MonoBehaviour
{
    private Rigidbody2D Rigidbody2D;
    [SerializeField] private float MinSpeed = 1000f;
    [SerializeField] private float MaxSpeed = 2000f;
    [SerializeField] private RectTransform Field;
    [SerializeField] private RectTransform MyRectTransform;
    public event System.Action<bool> OnHit;
    private float Speed;

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        MyRectTransform = GetComponent<RectTransform>();
        Field = transform.parent.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        NewSpawn();
    }

    private void FixedUpdate()
    {
        if(IsInField())
        {
            Rigidbody2D.linearVelocity = Vector3.left * Speed;
        }
        else
        {
            Rigidbody2D.linearVelocity = Vector3.left * 0;
            OnHit?.Invoke(false);
            NewSpawn();
        }
    }

    private bool IsInField()
    {
        Vector2 LocalPosition = Field.InverseTransformPoint(MyRectTransform.position);

        return Field.rect.Contains(LocalPosition);
    }

    private void NewSpawn()
    {
        Vector3[] Corners = new Vector3[4];
        Field.GetWorldCorners(Corners);
        float PositionY = Random.Range(Corners[3].y + MyRectTransform.rect.height, Corners[2].y - MyRectTransform.rect.height);
        float PositionX = Corners[3].x + MyRectTransform.rect.x;
        transform.position = new Vector3(PositionX, PositionY, 0);
        Speed = Random.Range(MinSpeed, MaxSpeed);
        MyRectTransform.localScale = Vector3.one * (1 + MapToRange(Speed, MinSpeed, MaxSpeed));
    }

    float MapToRange(float Value, float Min, float Max)
    {
        Value = Mathf.Clamp(Value, Min, Max);

        return (1 - (Value - Min) / (Max - Min));
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Spaceship"))
        {
            OnHit?.Invoke(true);
            NewSpawn();
        }
    }
}
