using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private int width = 21;
    [SerializeField] private int height = 11;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject startPrefab;
    [SerializeField] private GameObject goalPrefab;

    private int[,] maze;
    private Vector3 startMaze;

    private Vector2Int startPos;
    private Vector2Int goalPos;

    private void Start()
    {
        startMaze = transform.position;
        GenerateMaze();
        DrawMaze();
    }

    private void GenerateMaze()
    {
        width = (width % 2 == 0) ? width + 1 : width;
        height = (height % 2 == 0) ? height + 1 : height;

        maze = new int[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1;

        Carve(1, 1);

        startPos = new Vector2Int(1, 1);
        goalPos = new Vector2Int(width - 2, height - 2);
        maze[startPos.x, startPos.y] = 0;
        maze[goalPos.x, goalPos.y] = 0;
    }

    private void Carve(int x, int y)
    {
        maze[x, y] = 0;

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(2, 0),
            new Vector2Int(-2, 0),
            new Vector2Int(0, 2),
            new Vector2Int(0, -2)
        };

        Shuffle(directions);

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1)
            {
                if (maze[nx, ny] == 1)
                {
                    maze[x + dir.x / 2, y + dir.y / 2] = 0;
                    Carve(nx, ny);
                }
            }
        }
    }

    private void Shuffle(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Vector2Int temp = array[i];
            int randomIndex = Random.Range(i, array.Length);
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    private void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = startMaze + new Vector3(x, 0, y);

                if (maze[x, y] == 1)
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                }
                else if (x == startPos.x && y == startPos.y)
                {
                    Instantiate(startPrefab, pos, Quaternion.identity, transform);
                }
                else if (x == goalPos.x && y == goalPos.y)
                {
                    Instantiate(goalPrefab, pos, Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }

    public Vector3 GetGoalPosition()
    {
        return startMaze + new Vector3(goalPos.x, 0, goalPos.y);
    }
}
