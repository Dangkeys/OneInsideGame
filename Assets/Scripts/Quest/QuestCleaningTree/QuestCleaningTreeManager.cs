using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestCleaningTreeManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    private int currentScore = 0;
    [SerializeField] private int maxScore = 5;
    public event System.Action<bool> OnFinishedQuest;
    [SerializeField] private Transform allWormPosition;
    [SerializeField] private List<GameObject> wormholes = new List<GameObject>();

    private void Awake()
    {
        scoreBar = GetComponentInChildren<Scrollbar>();
        for (int i = 0; i < allWormPosition.childCount; i++)
        {
            wormholes.Add(allWormPosition.GetChild(i).gameObject);
        }
    }

    private void OnEnable()
    {
        currentScore = 0;
        UpdateScoreBar();
        RandomWormholeSpawn();
    }

    private void OnDisable()
    {
        SeTWormholeInactive();
    }

    public void IncreaseScore(int score)
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

    private void RandomWormholeSpawn()
    {
        List<int> indexs = new List<int>();
        for (int i = 0; i < wormholes.Count; i++)
        {
            indexs.Add(i);
        }

        for (int i = 0; i < maxScore; i++)
        {
            int index = Random.Range(0, indexs.Count);
            wormholes[indexs[index]].SetActive(true);
            indexs.Remove(index);
        }
    }

    private void SeTWormholeInactive()
    {
        foreach (GameObject wormhole in wormholes)
        {
            wormhole.SetActive(false);
        }
    }
}
