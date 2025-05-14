using UnityEngine;
using UnityEngine.UI;

public class QuestCrossCollectManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    public event System.Action<bool> OnFinishedQuest;
    [SerializeField] private int maxScore = 10;
    private int score = 0;
    [SerializeField] private CrossCubeController crossCubeController;

    private void Awake()
    {
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
    }

    private void OnEnable()
    {
        score = 0;
        UpdateScoreBar();
        crossCubeController.OnReached += OnReachedHandle;
    }

    private void OnDisable()
    {
        crossCubeController.OnReached -= OnReachedHandle;
    }

    private void OnReachedHandle(bool obj)
    {
        if (obj)
        {
            score++;
        }
        else
        {
            score = (score <= 0) ? 0 : score - 1;

        }
        UpdateScoreBar();
    }

    private void UpdateScoreBar()
    {
        float percent = (float)score / maxScore;
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
