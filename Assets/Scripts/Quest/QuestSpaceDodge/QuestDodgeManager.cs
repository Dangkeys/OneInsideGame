using System;
using UnityEngine;
using UnityEngine.UI;

public class QuestDodgeManager : MonoBehaviour
{
    [SerializeField] private Meteorite[] Meteorites;
    private Scrollbar ScoreBar;
    private int Score = 0;
    [SerializeField] private int MaxScore = 5;
    [SerializeField] private int LossScore = 1;
    private float Timer = 0f;
    [SerializeField] private float TimeGetScore = 1f;
    public event System.Action<bool> OnFinishedQuest;

    private void Awake()
    {
        Meteorites = GetComponentsInChildren<Meteorite>();
        ScoreBar = GetComponentInChildren<Scrollbar>();
    }

    private void OnEnable()
    {
        Score = 0;
        foreach (var Meteorite in Meteorites)
        {
            Meteorite.OnHit += HandleHit;
        }
    }

    private void OnDisable()
    {
        Score = 0;
        foreach (var Meteorite in Meteorites)
        {
            Meteorite.OnHit -= HandleHit;
        }
    }

    private void HandleHit(bool Hit)
    {
        if (Hit)
        {
            UpdateScoreBar(false);
        }
    }

    private void FixedUpdate()
    {
        Timer += Time.fixedDeltaTime;
        if(Timer > TimeGetScore)
        {
            UpdateScoreBar(true);
            Timer = 0f;
        }
    }

    private void UpdateScoreBar(bool GetScore)
    {
        if(GetScore)
        {
            Score++;
            if(Score >= MaxScore)
            {
                OnFinishedQuest?.Invoke(true);
            }
        }
        else
        {
            int NewScore = Score - LossScore;
            Score = Math.Max(0, NewScore);
        }
        ScoreBar.size = (float)Score / MaxScore;
    }
}
