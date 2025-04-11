using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestSentingEmailManager : MonoBehaviour
{
    [SerializeField] private EmailWord[] emailWords;
    private int index;
    [SerializeField] private TMP_Text showingText;
    private string writingWord;
    [SerializeField] private TMP_Text writingText;
    private float currentScore = 0;
    [SerializeField] private float percentWin = 0.8f;
    [SerializeField] private int winScore = 1;
    private int currentWin = 0;
    private Scrollbar scoreBar;
    public event System.Action<bool> OnFinishedQuest;

    private void Awake()
    {
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
    }

    private void OnEnable()
    {
        currentWin = 0;
        writingWord = string.Empty;
        writingText.text = writingWord;
        RandomIndex();
    }

    private void RandomIndex()
    {
        index = Random.Range(0, emailWords.Length);
        showingText.text = GetSelectedWord();
    }

    private string GetSelectedWord()
    {
        return emailWords[index].GetWord();
    }

    public void AddWritingWord(string word)
    {
        writingWord += word;
        writingText.text = writingWord;
    }

    public void DeleteWritingWord()
    {
        if (writingWord.Length > 0)
        {
            writingWord = writingWord.Remove(writingWord.Length - 1);
        }
        writingText.text = writingWord;
    }

    public void CheckWord()
    {
        currentScore = 0;
        int minLength = Mathf.Min(emailWords[index].GetLength(), writingWord.Length);

        for (int i = 0; i < minLength; i++)
        {
            if (emailWords[index].GetWord()[i] == writingWord[i])
            {
                currentScore++;
            }
        }

        if(currentScore > emailWords[index].GetWord().Length * percentWin)
        {
            UpdateScore(true);
        }

        writingWord = "";
        writingText.text = writingWord;
    }

    public void UpdateScore(bool getScore)
    {
        if (getScore)
        {
            currentWin++;
        }

        scoreBar.size = (float)currentWin / winScore;

        if (currentWin >= winScore)
        {
            HandleFinishedQuest();
        }
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }
}
