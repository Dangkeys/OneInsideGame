using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestDodgeManager : MonoBehaviour
{
    [SerializeField] private GameObject meteoritePrefab;
    [SerializeField] private int meteoriteAmount = 1;
    [SerializeField] private Transform meteoriteFolder;
    private List<Meteorite> meteorites = new List<Meteorite>();
    private Scrollbar scoreBar;
    private int currentScore = 0;
    [SerializeField] private int maxScore = 5;
    [SerializeField] private int loseScore = 1;
    private float currentTime = 0f;
    [SerializeField] private float timeToGetScore = 1f;
    public event System.Action<bool> OnFinishedQuest;
    [SerializeField] private GameObject spaceship;
    private Vector3 startPosition;

    private void Awake()
    {
        scoreBar = GetComponentInChildren<Scrollbar>();
        startPosition = spaceship.transform.position;
    }

    private void Start()
    {
        ObjectPooling();
    }

    private void OnEnable()
    {
        currentScore = 0;
        foreach (Meteorite meteorite in meteorites)
        {
            meteorite.onHit += HandleHit;
        }
        spaceship.transform.position = startPosition;
    }

    private void OnDisable()
    {
        currentScore = 0;
        UpdateScoreBar(false);
        foreach (Meteorite meteorite in meteorites)
        {
            meteorite.onHit -= HandleHit;
        }
    }

    private void ObjectPooling()
    {
        for(int i = 0; i < meteoriteAmount; i++)
        {
            GameObject obj = Instantiate(meteoritePrefab, meteoriteFolder);
            obj.SetActive(true);
            Meteorite newMeteorite = obj.GetComponent<Meteorite>();
            newMeteorite.onHit += HandleHit;
            meteorites.Add(newMeteorite);
        }
    }

    private void HandleHit(bool hit)
    {
        if (hit)
        {
            UpdateScoreBar(false);
        }
    }

    private void FixedUpdate()
    {
        currentTime += Time.fixedDeltaTime;
        if(currentTime > timeToGetScore)
        {
            UpdateScoreBar(true);
            currentTime = 0f;
        }
    }

    private void UpdateScoreBar(bool getScore)
    {
        if(getScore)
        {
            currentScore++;
            if(currentScore >= maxScore)
            {
                HandleFinishedQuest();
            }
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
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }
}
