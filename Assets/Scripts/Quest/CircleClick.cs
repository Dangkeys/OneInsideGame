using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CircleClick : MonoBehaviour
{
    private RectTransform Circle;
    private RectTransform Panel;
    private TMP_Text WordText;
    private float ScaleSize = 5f;
    [SerializeField] private float Size = 5f;
    [SerializeField] private float Duration = 2f;
    private float CurrentTime = 0;
    private int Score = 0;
    public event System.Action<string> OnWordChange;
    public event System.Action<bool> OnWin;
    private List<string> WordList = new List<string>();
    [SerializeField] private int WinScore = 1;
    [SerializeField] private Scrollbar ScoreBar;

    private void Awake()
    {
        Panel = GetComponent<RectTransform>();
        ScoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
        Transform CircleTransform = transform.Find("CircleClick");
        Circle = CircleTransform.GetComponent<RectTransform>();
        WordText = CircleTransform.Find("CircleText").GetComponent<TMP_Text>();
    }

    private void FixedUpdate()
    {
        CurrentTime += Time.fixedDeltaTime;
        float newSize = Mathf.Lerp(Size, 0, CurrentTime / Duration);
        SetScale(newSize);
        if(newSize <= 0)
        {
            UpdateScore(false);
        }
    }

    private void OnEnable()
    {
        Score = 0;
        SetCircle();
    }

    private void OnDisable()
    {
        Score = 0;
    }

    public void UpdateScore(bool GetScore)
    {
        if (GetScore)
        {
            Score++;
        }
        else if (Score > 0)
        {
            Score--;
        }
        ScoreBar.size = (float)Score / WinScore;
        if (Score >= WinScore)
        {
            HandleWin();
        }
        else
        {
            SetCircle();
        }
    }

    public void AddWordList(string Word)
    {
        WordList.Add(Word);
    }

    public void SetCircle()
    {
        CurrentTime = 0f;
        RandomPosition();
        SetWord();
        SetScale(Size);
    }

    private void RandomPosition()
    {
        float PanelWidth = Panel.rect.width;
        float PanelHeight = Panel.rect.height;
        float CircleWidth = Circle.rect.width;
        float CircleHeight = Circle.rect.height;
        Vector3 Position = new Vector3(Random.Range((-PanelWidth + CircleWidth * Size) / 2, (PanelWidth - CircleWidth * Size) / 2), 
            Random.Range((-PanelHeight + CircleHeight * Size) / 2, (PanelHeight - CircleHeight * Size) / 2), 0);
        Circle.transform.localPosition = Position;
    }

    private void SetScale(float Size)
    {
        ScaleSize = Size;
        Circle.transform.localScale = new Vector3(ScaleSize, ScaleSize);
    }

    private void SetWord()
    {
        int RandomIndex = Random.Range(0, WordList.Count);
        string NewWord = WordList[RandomIndex];
        HandleWordChanged(NewWord);
        WordText.text = NewWord;
    }

    private void HandleWordChanged(string Word)
    {
        OnWordChange?.Invoke(Word);
    }

    private void HandleWin()
    {
        OnWin?.Invoke(true);
    }
}
