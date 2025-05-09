using UnityEngine;
using UnityEngine.UI;

public class QuestShellGameManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    public event System.Action<bool> OnFinishedQuest;
    private int currentScore = 0;
    [SerializeField] private int maxScore = 5;

    private void Awake()
    {
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
    }

    private void OnEnable()
    {
        scoreBar.size = 0f;
        currentScore = 0;
    }

    public void UpdateScore(int score)
    {
        currentScore += score;
        if(currentScore < 0)
        {
            currentScore = 0;
        }
        UpdateScoreBar();
    }

    private void UpdateScoreBar()
    {
        float percent = (float)currentScore / maxScore;
        scoreBar.size = percent;
        if (percent >= 1f)
        {
            HandleFinishedQuest();
        }
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }
}
