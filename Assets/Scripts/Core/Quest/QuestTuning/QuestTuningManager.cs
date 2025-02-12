using UnityEngine;
using UnityEngine.UI;

public class QuestTuningManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    private int currentScore = 0;
    [SerializeField] private int maxScore = 5;
    public event System.Action<bool> OnFinishedQuest;
    private int maxAmountDirection;
    private int currentAmountDirection = 0;
    [SerializeField] private float percentWin = 0.9f;

    private void Awake()
    {
        scoreBar = GetComponentInChildren<Scrollbar>();
    }

    private void OnEnable()
    {
        currentScore = 0;
        currentAmountDirection = 0;
        UpdateScoreBar();
    }

    public void IncreateAmountDirection(int amount)
    {
        currentAmountDirection += amount;
    }

    private void IncreaseScore(int score)
    {
        currentScore += score;
        UpdateScoreBar();
        if (currentScore >= maxScore)
        {
            HandleFinishedQuest();
        }
    }

    private void UpdateScoreBar()
    {
        scoreBar.size = (float)currentScore / maxScore;
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }

    public void SetMaxDirection(int amount)
    {
        maxAmountDirection = amount;
    }

    public void FinishRound()
    {
        if(currentAmountDirection * percentWin >= maxAmountDirection)
        {
            IncreaseScore(1);
        }
    }
}
