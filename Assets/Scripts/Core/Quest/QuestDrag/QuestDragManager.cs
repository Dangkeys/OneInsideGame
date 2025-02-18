using UnityEngine;
using UnityEngine.UI;

public class QuestDragManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    private CircleTarget[] circleTargets;
    private RectTransform[] rectCircleTargets;
    private RectTransform rectPanel;
    private int currentScore = 0;
    [SerializeField]private int maxScore = 10;
    [SerializeField] private int loseScore = 1;
    public event System.Action<bool> OnFinishedQuest;

    private void Awake()
    {
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
        rectPanel = GetComponent<RectTransform>();
        circleTargets = GetComponentsInChildren<CircleTarget>();
        rectCircleTargets = new RectTransform[circleTargets.Length];

        for (int i = 0; i < circleTargets.Length; i++)
        {
            rectCircleTargets[i] = circleTargets[i].GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < circleTargets.Length; i++)
        {
            int index = i;
            circleTargets[i].onHit += (hit) => HandleHit(index, hit);
            RandomPositionTarget(i);
        }
    }

    private void OnDisable()
    {
        currentScore = 0;
        UpdateScoreBar(false);
        for (int i = 0; i < circleTargets.Length; i++)
        {
            circleTargets[i].onHit -= (hit) => HandleHit(i, hit);
        }
    }

    private void HandleHit(int index, bool hit)
    {
        RandomPositionTarget(index);
        UpdateScoreBar(hit);
    }

    private void RandomPositionTarget(int index)
    {
        float panelWidth = rectPanel.rect.width;
        float panelHeight = rectPanel.rect.height;
        float circleWidth = rectCircleTargets[index].rect.width;
        float circleHeight = rectCircleTargets[index].rect.height;

        Vector3 position;
        bool positionIsValid;

        do
        {
            position = new Vector3(Random.Range((-panelWidth + circleWidth) / 2, (panelWidth - circleWidth) / 2),
                Random.Range((-panelHeight + circleHeight) / 2, (panelHeight - circleHeight) / 2), 0);

            positionIsValid = true;

            for (int i = 0; i < rectCircleTargets.Length; i++)
            {
                float distance = Vector3.Distance(position, rectCircleTargets[i].transform.localPosition);
                float minDistance = (circleWidth + rectCircleTargets[i].rect.width) / 2;

                if (distance < minDistance)
                {
                    positionIsValid = false;
                    break;
                }
            }
        }
        while (!positionIsValid); 

        rectCircleTargets[index].transform.localPosition = position;
    }

    private void UpdateScoreBar(bool getScore)
    {
        if(getScore)
        {
            currentScore++;
        }
        else if(currentScore > loseScore) 
        {
            currentScore -= loseScore;
        }
        else
        {
            currentScore = 0;
        }

        scoreBar.size = (float)currentScore / maxScore;

        if (currentScore >= maxScore)
        {
            HandleFinishedQuest();
        }
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }
}
