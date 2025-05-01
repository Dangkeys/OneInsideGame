using UnityEngine;

public class ShellBall : MonoBehaviour
{
    private enum ShellBallState
    {
        Waiting,
        GoStraight,
        GoAside
    }
    private Vector3 goStraight;
    private Vector3 goAside;
    private ShellBallState state = ShellBallState.Waiting;
    [SerializeField] private float speed = 1f;
    private Vector3 initPosition;

    private void Awake()
    {
        initPosition = transform.position;
    }

    private void OnEnable()
    {
        transform.position = initPosition;
        state = ShellBallState.Waiting;
    }

    private void Update()
    {
        switch (state)
        {
            case ShellBallState.GoStraight:
                GoForward(goStraight, ShellBallState.Waiting);
                break;
            case ShellBallState.GoAside:
                GoForward(goAside, ShellBallState.GoStraight);
                break;
            default:
                break;
        }
    }

    public void GoDown(Vector3 position, float distance)
    {
        goAside = new Vector3(position.x, transform.position.y, transform.position.z);
        goStraight = new Vector3(position.x, transform.position.y - distance, transform.position.z);
        state = ShellBallState.GoAside;
    }

    public void GoUp(Vector3 position, float distance)
    {
        transform.position = new Vector3(position.x, transform.position.y, transform.position.z);
        goStraight = new Vector3(position.x, transform.position.y + distance, transform.position.z);
        state = ShellBallState.GoStraight;
    }

    public bool IsInPosition()
    {
        return state == ShellBallState.Waiting;
    }

    private void GoForward(Vector3 direction, ShellBallState shellBallState)
    {
        transform.position = Vector3.MoveTowards(transform.position, direction, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, direction) < 0.01f)
        {
            state = shellBallState;
        }
    }
}
