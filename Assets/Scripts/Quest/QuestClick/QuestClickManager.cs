using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestClickManager : MonoBehaviour
{
    private RectTransform rectCircle;
    private RectTransform rectPanel;
    private TMP_Text wordText;
    private float currentScale;
    [SerializeField] private float scale = 5f;
    [SerializeField] private float duration = 2f;
    private float currentTime = 0;
    private int currentScore = 0;
    public event System.Action<string> onChangedWord;
    public event System.Action<bool> OnFinishedQuest;
    private List<string> wordList = new List<string>();
    [SerializeField] private int winScore = 10;
    private Scrollbar scoreBar;

    private void Awake()
    {
        rectPanel = GetComponent<RectTransform>();
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
        Transform circleTransform = transform.Find("CircleClick");
        rectCircle = circleTransform.GetComponent<RectTransform>();
        wordText = circleTransform.Find("CircleText").GetComponent<TMP_Text>();
    }

    private void FixedUpdate()
    {
        currentTime += Time.fixedDeltaTime;
        float newSize = Mathf.Lerp(scale, 0, currentTime / duration);
        SetScale(newSize);
        if(newSize <= 0)
        {
            UpdateScore(false);
        }
    }

    private void OnEnable()
    {
        currentScore = 0;
        SetNewCircle();
    }

    private void OnDisable()
    {
        currentScore = 0;
    }

    public void UpdateScore(bool getScore)
    {
        if (getScore)
        {
            currentScore++;
        }
        else if (currentScore > 0)
        {
            currentScore--;
        }

        scoreBar.size = (float)currentScore / winScore;

        if (currentScore >= winScore)
        {
            HandleFinishedQuest();
        }
        else
        {
            SetNewCircle();
        }
    }

    public void AddWordList(string word)
    {
        wordList.Add(word);
    }

    public void SetNewCircle()
    {
        currentTime = 0f;
        RandomPosition();
        SetNewWord();
        SetScale(scale);
    }

    private void RandomPosition()
    {
        float panelWidth = rectPanel.rect.width;
        float panelHeight = rectPanel.rect.height;
        float circleWidth = rectCircle.rect.width;
        float circleHeight = rectCircle.rect.height;
        Vector3 position = new Vector3(Random.Range((-panelWidth + circleWidth * scale) / 2, (panelWidth - circleWidth * scale) / 2), 
            Random.Range((-panelHeight + circleHeight * scale) / 2, (panelHeight - circleHeight * scale) / 2), 0);
        rectCircle.transform.localPosition = position;
    }

    private void SetScale(float newScale)
    {
        currentScale = newScale;
        rectCircle.transform.localScale = new Vector3(currentScale, currentScale);
    }

    private void SetNewWord()
    {
        int randomIndex = Random.Range(0, wordList.Count);
        string newWord = wordList[randomIndex];
        HandleChangedWord(newWord);
        wordText.text = newWord;
    }

    private void HandleChangedWord(string word)
    {
        onChangedWord?.Invoke(word);
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }
}
