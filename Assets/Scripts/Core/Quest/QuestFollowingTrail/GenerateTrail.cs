using UnityEngine;

public class GenerateTrail : MonoBehaviour
{
    [SerializeField] private GameObject skyPrefab;
    [SerializeField] private GameObject trailPrefab;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;

    private int[,] area;
    private Vector3 startPosition;

    private bool bend = false;
    private int currentWidth = 0;
    private int currentHeight = 0;

    [SerializeField] private GameObject startPrefab;
    [SerializeField] private GameObject finishPrefab;

    private void Awake()
    {
        startPosition = transform.position;
        GenerateArea();
        GeneratePath();
        InstantiateTiles();
        AddCollider();
    }

    private void GenerateArea()
    {
        area = new int[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                area[x, y] = 1;
    }

    private void GeneratePath()
    {
        currentHeight = Random.Range(0, height);
        currentWidth = 0;
        area[currentWidth, currentHeight] = 0;

        while (currentWidth < width - 1)
        {
            if (bend)
            {
                BendWay();
            }
            else
            {
                StraightWay();
            }
        }
    }

    private void StraightWay()
    {
        bend = true;

        int maxStep = width - currentWidth - 1;
        int step = Random.Range(2, Mathf.Min(5, maxStep + 1)); 

        for (int i = 0; i < step; i++)
        {
            currentWidth++;
            area[currentWidth, currentHeight] = 0;

            if (currentWidth >= width - 1)
                break;
        }
    }


    private void BendWay()
    {
        bend = false;

        int direction = Random.Range(0, 2) == 0 ? -1 : 1; 
        int step = Random.Range(2, 5); 

        for (int i = 0; i < step; i++)
        {
            currentHeight += direction;

            if (currentHeight < 0)
            {
                currentHeight = 0;
                break;
            }
            else if (currentHeight >= height)
            {
                currentHeight = height - 1;
                break;
            }

            area[currentWidth, currentHeight] = 0;
        }
    }

    private void InstantiateTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0)
                {
                    GameObject gameObject = Instantiate(startPrefab, startPosition + new Vector3(x, 0, y), Quaternion.identity, transform);
                    gameObject.SetActive(true);
                }
                else if (x >= width - 1)
                {
                    GameObject gameObject = Instantiate(finishPrefab, startPosition + new Vector3(x, 0, y), Quaternion.identity, transform);
                    gameObject.SetActive(true);
                }
                else
                {
                    GameObject prefab = (area[x, y] == 0) ? trailPrefab : skyPrefab;
                    GameObject gameObject = Instantiate(prefab, startPosition + new Vector3(x, 0, y), Quaternion.identity, transform);
                    gameObject.SetActive(true);
                }
            }
        }
    }

    private void AddCollider()
    {
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();

        boxCollider.size = new Vector3(width, 1, height);

        boxCollider.center = new Vector3((width - 1) / 2f, 0, (height - 1) / 2f);
    }
}
