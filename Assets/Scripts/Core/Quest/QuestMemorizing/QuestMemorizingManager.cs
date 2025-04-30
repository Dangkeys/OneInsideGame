using UnityEngine;
using UnityEngine.UI;

public class QuestMemorizingManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    private uint currentScore = 0;
    private uint maxScore;
    public event System.Action<bool> OnFinishedQuest;

    private void Awake()
    {
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
    }

    private void OnEnable()
    {
        currentScore = 0;
        UpdateScoreBar();
    }

    private void UpdateScoreBar()
    {
        scoreBar.size = (float)currentScore / maxScore;
    }

    public void SetMaxScore(uint score)
    {
        maxScore = score;
    }

    public void AddScore(uint score)
    {
        currentScore += score;
        UpdateScoreBar();
        if(currentScore >= maxScore)
        {
            HandleFinishedQuest();
        }
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }
}
