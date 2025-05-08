using TMPro;
using UnityEngine;

public class TextAnimation : MonoBehaviour
{
    [SerializeField] private string initText;
    [SerializeField] private string text;
    private TMP_Text tmpText;
    private int index = 0;
    private float currentTime = 0f;

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        tmpText.text = initText;
    }

    private void Update()
    {
        if(currentTime > 1f)
        {
            currentTime = 0f;
            AddText();
        }
        else
        {
            currentTime += Time.deltaTime;
        }
    }

    public void AddText()
    {
        index = (index + 1) % (text.Length + 1);
        tmpText.text = initText + text.Substring(0, index);
    }
}
