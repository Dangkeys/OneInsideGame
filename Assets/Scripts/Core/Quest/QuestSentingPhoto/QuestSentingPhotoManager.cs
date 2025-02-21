using UnityEngine;
using UnityEngine.UI;

public class QuestSentingPhotoManager : MonoBehaviour
{
    [SerializeField] private ImageCapture[] imageCaptures;
    private int index;
    private Scrollbar scoreBar;
    public event System.Action<bool> OnFinishedQuest;
    [SerializeField] private Image image;
    private float currentTime = 0;
    [SerializeField] private float timeToFinished = 5000f;
    private bool isImageCorrect = false;
    public event System.Action<bool> OnImageCorrect;
    [SerializeField] private GameObject loadScreen;

    private void Awake()
    {
        scoreBar = transform.Find("ScoreBar").GetComponent<Scrollbar>();
    }

    private void OnEnable()
    {
        isImageCorrect = false;
        currentTime = 0f;
        loadScreen.SetActive(false);
        RandomIndex();
        UpdateScoreBar();
    }

    private void RandomIndex()
    {
        index = Random.Range(0, imageCaptures.Length);
        image.sprite = imageCaptures[index].GetImage();
    }

    public void CheckPhoto(string name)
    {
        if(name == imageCaptures[index].GetName())
        {
            isImageCorrect = true;
            OnImageCorrect?.Invoke(true);
            loadScreen.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isImageCorrect)
            return;
        if(currentTime >= timeToFinished)
        {
            HandleFinishedQuest();
        }
        else
        {
            currentTime += Time.deltaTime;
            UpdateScoreBar();
        }
    }

    private void UpdateScoreBar()
    {
        scoreBar.size = currentTime / timeToFinished;
    }

    private void HandleFinishedQuest()
    {
        OnFinishedQuest?.Invoke(true);
    }
}
