using UnityEngine;

public class ShellCup : MonoBehaviour
{
    private enum ShellCupState
    {
        Waiting,
        GoAside,
        GoForward,
        GoBack
    }

    ShellCupState state = ShellCupState.Waiting;
    private Vector3 target;
    private float speed;
    [SerializeField]private int index;
    [SerializeField] private int currentIndex;
    private int direction;

    private float distance;
    private float duration;

    private Vector3 initPosition;

    private void Awake()
    {
        initPosition = transform.position;
    }

    public void SetPositionToDefault()
    {
        transform.position = initPosition;
        currentIndex = index;
    }

    private void Update()
    {
        switch (state)
        {
            case ShellCupState.GoAside:
                GoToTarget(GoForward);
                break;
            case ShellCupState.GoForward:
                GoToTarget(GoBack);
                break;
            case ShellCupState.GoBack:
                GoToTarget(() => state = ShellCupState.Waiting);
                break;
        }
    }

    public void SetIndex(int i)
    {
        index = i;
        currentIndex = i;
    }

    public void SetCurrentIndex(int i)
    {
        currentIndex = i;
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void GoToIndex(float moveDistance, float time, int dir)
    {
        direction = dir;
        distance = Mathf.Abs(moveDistance);
        duration = time / 4;
        if(moveDistance < 0)
        {
            GoLeft();
        }
        else
        {
            GoRight();
        }
    }

    private void GoRight()
    {
        target = transform.position + Vector3.forward * distance;
        speed = distance / duration;
        state = ShellCupState.GoAside;
    }

    private void GoLeft()
    {
        target = transform.position + Vector3.back * distance;
        speed = distance / duration;
        state = ShellCupState.GoAside;
    }

    private void GoForward()
    {
        target = transform.position + Vector3.right * distance * direction;
        speed = distance / duration;
        state = ShellCupState.GoForward;
    }

    private void GoBack()
    {
        target = new Vector3(transform.position.x, transform.position.y, initPosition.z);
        float backDistance = Vector3.Distance(transform.position, target);
        speed = backDistance / duration;
        state = ShellCupState.GoBack;
    }

    private void GoToTarget(System.Action onReach)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            onReach?.Invoke();
        }
    }

    public bool IsIdle()
    {
        return state == ShellCupState.Waiting;
    }
}

