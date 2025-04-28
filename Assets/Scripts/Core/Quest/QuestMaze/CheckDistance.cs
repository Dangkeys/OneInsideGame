using UnityEngine;

public class CheckDistance : MonoBehaviour
{
    [SerializeField] private MazeGenerator mazeGenerator;
    private Vector3 goal;
    [SerializeField] private QuestEscapingManager questEscapingManager;
    private Rigidbody rb;
    private float maxDistance;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        goal = mazeGenerator.GetGoalPosition();
        maxDistance = Vector3.Distance(goal, transform.position);
    }

    private void Update()
    {
        if (rb.linearVelocity != Vector3.zero)
        {
            float distance = Vector3.Distance(goal, transform.position);
            questEscapingManager.UpdateScoreBar(distance, maxDistance);
        }
    }
}
