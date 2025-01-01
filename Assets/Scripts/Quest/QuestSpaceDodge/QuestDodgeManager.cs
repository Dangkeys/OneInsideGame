using System;
using UnityEngine;
using UnityEngine.UI;

public class QuestDodgeManager : MonoBehaviour
{
    private Meteorite[] meteorites;
    private Scrollbar scoreBar;
    private int currentScore = 0;
    [SerializeField] private int maxScore = 5;
    [SerializeField] private int loseScore = 1;
    private float currentTime = 0f;
    [SerializeField] private float timeToGetScore = 1f;
    public event System.Action<bool> OnFinishedQuest;

    private void Awake()
    {
        meteorites = GetComponentsInChildren<Meteorite>();
        scoreBar = GetComponentInChildren<Scrollbar>();
    }

    private void OnEnable()
    {
        currentScore = 0;
        foreach (Meteorite meteorite in meteorites)
        {
            meteorite.onHit += HandleHit;
        }
    }

    private void OnDisable()
    {
        currentScore = 0;
        foreach (Meteorite meteorite in meteorites)
        {
            meteorite.onHit -= HandleHit;
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
                OnFinishedQuest?.Invoke(true);
            }
        }
        else
        {
            int newScore = currentScore - loseScore;
            currentScore = Math.Max(0, newScore);
        }
        scoreBar.size = (float)currentScore / maxScore;
    }
}
