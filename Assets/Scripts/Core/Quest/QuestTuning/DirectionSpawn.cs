using UnityEngine;

public class DirectionSpawn : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private GameObject direction;
    private Direction[] directions;
    [SerializeField] private Transform directionFolder;
    [SerializeField] private float distance = 100f;
    private float useDistance = 0f;
    private int index = 0;
    private Vector3 currentPosition;
    private float allDistance = 0f;
    private bool isRandomFinish = false;
    [SerializeField] private QuestTuningManager questTuningManager;
    [SerializeField] private Wave wave;
    private float size;
    private bool isFirstSpawn = true;
    private void Awake()
    {
        size = direction.GetComponent<RectTransform>().rect.width * direction.GetComponent<RectTransform>().localScale.x;
        int count = Mathf.FloorToInt(panel.rect.width / size) - 2;
        directions = new Direction[count];
        for (int i = 0; i < count; i++)
        {
            GameObject newDirection = Instantiate(direction, directionFolder);
            directions[i] = newDirection.GetComponent<Direction>();
        }
        questTuningManager.SetMaxDirection(count);
    }

    private void OnEnable()
    {
        foreach (Direction direction in directions)
        {
            direction.OnConnect += OnConnect;
        }
        wave.OnEndDirection += OnEndDirection;
        currentPosition = panel.position - new Vector3(panel.rect.width / 2, 0, 0);
        isRandomFinish = false;
        allDistance = 0;
        isFirstSpawn = true;
        RandomPosition();
    }

    private void OnDisable()
    {
        foreach (Direction direction in directions)
        {
            direction.OnConnect -= OnConnect;
        }
        wave.OnEndDirection -= OnEndDirection;
    }

    private void OnConnect(bool connect)
    {
        if(connect)
        {
            questTuningManager.IncreateAmountDirection(1);
        }
    }
    private void OnEndDirection(bool end)
    {
        if (end)
        {
            currentPosition = panel.position - new Vector3(panel.rect.width / 2, 0, 0);
            isRandomFinish = false;
            allDistance = 0;
            isFirstSpawn = true;
            RandomPosition();
            questTuningManager.FinishRound();
        }
    }

    private void RandomPosition()
    {
        while (!isRandomFinish)
        {
            bool isHorizontal = (Random.Range(0, 2) == 0) ? true : false  || isFirstSpawn;
            isFirstSpawn = false;
            useDistance = 0f;
            while (useDistance < distance)
            {
                if (allDistance > panel.rect.width - 2 * size)
                {
                    isRandomFinish = true;
                    break;
                }
                currentPosition += new Vector3(size, 0, 0);
                if (!isHorizontal)
                {
                    if (useDistance < distance / 2 && currentPosition.y < panel.position.y + panel.rect.height / 4)
                    {
                        currentPosition += new Vector3(0, directions[index].GetHeight(), 0);
                    }
                    else if (currentPosition.y > panel.position.y - panel.rect.height / 4 )
                    {
                        currentPosition -= new Vector3(0, directions[index].GetHeight(), 0);
                    }
                }
                useDistance += size;
                allDistance += size;
                directions[index].SetSpawn(currentPosition);
                index = (index + 1) % directions.Length;
            }
        }
    }
}
