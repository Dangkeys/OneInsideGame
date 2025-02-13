using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BootstrapUI : MonoBehaviour
{
    private TextMeshProUGUI bootstrapText;
    private Slider progressSlider;
    void Start()
    {
        bootstrapText = GetComponentInChildren<TextMeshProUGUI>();
        progressSlider = GetComponentInChildren<Slider>();
        OneInsideGameManager.Instance.OnBootstrapLoadingProgressChanged += UpdateProgress;
    }

    private void UpdateProgress(float progressValue, string progressText)
    {
        bootstrapText.text = progressText;
        progressSlider.value = progressValue;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
