using System;
using UnityEngine;
using UnityEngine.UI;

public class QuestShootingManager : MonoBehaviour
{
    private Scrollbar scoreBar;
    private float currentTime = 0f;
    [SerializeField] private float winTime = 20f;
    public event System.Action<bool> OnFinishedQuest;
    [SerializeField] private int maxHeart = 3;
    private int heart;
    private int index = 0;
    [SerializeField] private GameObject heartGameObject;
    [SerializeField] private Transform heartStorage;
    private GameObject[] hearts;

    private void Awake()
    {
        scoreBar = GetComponentInChildren<Scrollbar>();
        hearts = new GameObject[maxHeart];
        for (int i = 0; i < maxHeart; i++)
        {
            GameObject newHeart = Instantiate(heartGameObject, heartStorage);
            hearts[i] = newHeart;
        }
    }

    private void OnEnable()
    {
        index = 0;
        currentTime = 0f;
        heart = maxHeart;
        foreach (GameObject heart in hearts)
        {
            heart.SetActive(true);
        }
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        UpdateScoreBar();
        
    }

    private void UpdateScoreBar()
    {
        scoreBar.size = currentTime / winTime;
        if(currentTime >= winTime)
        {
            HandleFinishedQuest(true);
        }
    }

    private void HandleFinishedQuest(bool finish)
    {
        OnFinishedQuest?.Invoke(finish);
    }

    public void TakeDamage()
    {
        heart -= 1;
        hearts[index].SetActive(false);
        index++;
        if (heart <= 0)
        {
            HandleFinishedQuest(false);
            gameObject.SetActive(false);
        }
    }
}
