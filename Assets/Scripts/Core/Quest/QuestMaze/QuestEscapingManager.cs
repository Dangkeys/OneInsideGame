using UnityEngine;
using UnityEngine.UI;

public class QuestEscapingManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    public event System.Action<bool> OnFinishedQuest;

    private void Awake()
    {
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
    }

    private void OnEnable()
    {
        scoreBar.size = 0f;
    }

    public void UpdateScoreBar(float distance, float maxDistance)
    {
        scoreBar.size = (maxDistance - distance) / maxDistance;
        if(scoreBar.size >= 0.95f)
        {
            HandleFinishedQuest();
        }
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }
}
