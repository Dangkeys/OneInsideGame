using UnityEngine;

public class ShowOffForCube : MonoBehaviour
{
    private Vector3 initPosition;
    public event System.Action<bool> OnStart;
    public event System.Action<bool> OnNewGame;
    [SerializeField] private QuestFollowingTrailManager questFollowingTrailManager;
    private void Awake()
    {
        initPosition = transform.position;
    }

    private void OnEnable()
    {
        OnNewGame?.Invoke(true);
    }

    private void NewGame()
    {
        transform.position = initPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Sky"))
        {
            NewGame();
        }
        else if(collision.collider.CompareTag("Goal"))
        {
            questFollowingTrailManager.Win();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.collider.CompareTag("Start"))
        {
            OnStart?.Invoke(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Start"))
        {
            OnStart?.Invoke(false);
        }
    }
}
