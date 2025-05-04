using System;
using System.Collections;
using UnityEngine;

public class ButtonShellGameHandle : MonoBehaviour
{
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject fillBallButton;
    [SerializeField] private GameObject checkBallButton;
    [SerializeField] private ShellCupManager shellCupManager;

    private void OnEnable()
    {
        SetupInitialUI();
        shellCupManager.OnSuffleFinished += ShuffleHandle;
    }

    private void OnDisable()
    {
        shellCupManager.OnSuffleFinished -= ShuffleHandle;
    }

    private void ShuffleHandle(bool isFinished)
    {
        if(isFinished)
        {
            SetButtonActive(checkBallButton, true);
        }
    }

    private void SetupInitialUI()
    {
        SetButtonActive(startButton, false);
        SetButtonActive(fillBallButton, true);
        SetButtonActive(checkBallButton, false);
    }

    public void FillBall(int index)
    {
        StartCoroutine(FillBallRoutine(index));
    }

    private IEnumerator FillBallRoutine(int index)
    {
        SetButtonActive(fillBallButton, false);
        yield return StartCoroutine(shellCupManager.FillBall(index));
        SetButtonActive(startButton, true);
    }

    public void CheckBall(int index)
    {
        StartCoroutine(CheckBallRoutine(index));
    }

    private IEnumerator CheckBallRoutine(int index)
    {
        SetButtonActive(checkBallButton, false);
        yield return StartCoroutine(shellCupManager.CheckBall(index));
        SetButtonActive(fillBallButton, true);
    }

    public void StartGame()
    {
        shellCupManager.ShuffleButtle();
        SetButtonActive(startButton, false);
    }

    private void SetButtonActive(GameObject button, bool active)
    {
        if (button != null)
            button.SetActive(active);
    }
}
