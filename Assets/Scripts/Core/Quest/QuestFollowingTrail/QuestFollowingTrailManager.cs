using UnityEngine;
using UnityEngine.UI;

public class QuestFollowingTrailManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    public event System.Action<bool> OnFinishedQuest;
    private int blockCount = 0;
    private int currentBlock = 0;

    private void Awake()
    {
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
    }

    private void OnEnable()
    {
        scoreBar.size = 0f;
        currentBlock = 0;
    }

    public void AddAllBlock(int block)
    {
        blockCount += block;
    }

    public void GetNewBlock(int block)
    {
        currentBlock += block;
        UpdateScoreBar();
    }

    private void UpdateScoreBar()
    {
        float percent = (float)currentBlock / blockCount;
        scoreBar.size = percent;
        if (percent >= 1f)
        {
            HandleFinishedQuest();
        }
    }

    public void Win()
    {
        HandleFinishedQuest();
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }
}
