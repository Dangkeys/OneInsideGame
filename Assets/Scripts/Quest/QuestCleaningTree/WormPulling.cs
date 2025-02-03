using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WormPulling : MonoBehaviour, IPointerDownHandler
{
    private Vector2 myTransform;
    private bool isDrag = false;
    private Vector2 mousePosition;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float maxDistanceDragInOneRound = 10f;
    private float distanceDragInOneRound = 0f;
    [SerializeField] private float distanceToWin = 80f;
    private float currentDistance;
    [SerializeField] private GameObject wormhole;
    [SerializeField] private QuestCleaningTreeManager questCleaningTreeManager;

    private void Awake()
    {
        myTransform = transform.position;
    }

    private void OnEnable()
    {
        transform.position = myTransform;
        currentDistance = 0f;
        distanceDragInOneRound = 0f;
    }

    private void Update()
    {
        if (isDrag)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            if(currentMousePosition.x < mousePosition.x)
            {
                Vector2 newPosition = new Vector3(transform.position.x - speed, transform.position.y);
                transform.position = newPosition;
                UpdateDistance();
                UpdateDistanceCanDrag();
            }
            mousePosition = currentMousePosition;
        }
    }

    private void UpdateDistance()
    {
        currentDistance += speed;
        if (currentDistance >= distanceToWin)
        {
            questCleaningTreeManager.IncreaseScore(1);
            wormhole.SetActive(false);
        }
    }

    private void UpdateDistanceCanDrag()
    {
        distanceDragInOneRound -= speed;
        if (distanceDragInOneRound < 0f)
        {
            isDrag = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = true;
        mousePosition = Mouse.current.position.ReadValue();
        distanceDragInOneRound = maxDistanceDragInOneRound;
    }
}
