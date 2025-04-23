using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestDragManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    private List<CircleTarget> circleTargets = new List<CircleTarget>();
    private List<RectTransform> rectCircleTargets = new List<RectTransform>();
    private RectTransform rectPanel;
    private int currentScore = 0;
    [SerializeField]private int maxScore = 10;
    [SerializeField] private int loseScore = 1;
    public event System.Action<bool> OnFinishedQuest;
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private Transform circleLocation;
    [SerializeField] private int circleAmount = 3;

    private void Awake()
    {
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
        rectPanel = GetComponent<RectTransform>();
    }

    private void Start()
    {
        ObjectPooling();
    }

    private void ObjectPooling()
    {
        for(int i = 0; i < circleAmount; i++)
        {
            GameObject obj = Instantiate(circlePrefab, circleLocation);
            obj.SetActive(true);
            CircleTarget target = obj.GetComponent<CircleTarget>();
            int index = i;
            target.onHit += (hit) => HandleHit(index, hit);
            circleTargets.Add(target);
            RectTransform rect = target.GetComponent<RectTransform>();
            rectCircleTargets.Add(rect);
            RandomPositionTarget(i);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < circleAmount; i++)
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
        for (int i = 0; i < circleAmount; i++)
        {
            int index = i;
            circleTargets[i].onHit -= (hit) => HandleHit(index, hit);
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

            for (int i = 0; i < rectCircleTargets.Count; i++)
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
