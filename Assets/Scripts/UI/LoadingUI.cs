using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    private TextMeshProUGUI bootstrapText;
    private Slider progressSlider;
    private UIManager uIManager;

    void Awake()
    {
        bootstrapText = GetComponentInChildren<TextMeshProUGUI>();
        progressSlider = GetComponentInChildren<Slider>();

    }
    void Start()
    {
        uIManager = UIManager.Instance;
        uIManager.OnLoadingProgressChanged += UpdateProgress;

        gameObject.SetActive(false);
    }
    void Update()
    {

    }

    void OnDestroy()
    {

        uIManager.OnLoadingProgressChanged -= UpdateProgress;

    }

    private void UpdateProgress(float progressValue, string progressText)
    {
        bootstrapText.text = progressText;
        progressSlider.value = progressValue;

        if (progressValue < 1)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
