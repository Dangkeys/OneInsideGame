using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    private TextMeshProUGUI bootstrapText;
    private Slider progressSlider;

    void Start()
    {
        bootstrapText = GetComponentInChildren<TextMeshProUGUI>();
        progressSlider = GetComponentInChildren<Slider>();
        OneInsideGameManager.Instance.OnLoadingProgressChanged += UpdateProgress;
        gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
       OneInsideGameManager.Instance.OnLoadingProgressChanged -= UpdateProgress; 
    }

    private void UpdateProgress(float progressValue, string progressText)
    {
        if(progressValue == 1)
        {
            gameObject.SetActive(false);
    
        }
        gameObject.SetActive(true);
        bootstrapText.text = progressText;
        progressSlider.value = progressValue;
    }
}
