using System.Collections;
using UnityEngine;

public class ShellCupManager : MonoBehaviour
{
    private enum ShellCupManageState {
        Waiting,
        Shuffle,
        Guessing,
        Filling
    }

    [SerializeField] private uint amountShuffle = 5;
    private uint currentShuffle = 0;
    private ShellCupManageState state = ShellCupManageState.Waiting;
    [SerializeField] private ShellCup[] shellCups;
    [SerializeField] private float distance = 3;
    [SerializeField] private float maxTime = 1f;
    [SerializeField] private float minimalTime = 0.5f;
    [SerializeField] private ShellBall shellBall;
    private int ballInCup = 0;
    [SerializeField] private QuestShellGameManager questShellGameManager;
    public event System.Action<bool> OnSuffleFinished;

    private void OnEnable()
    {
        for (int i = 0; i < shellCups.Length; i++)
        {
            shellCups[i].SetIndex(i);
        }
        state = ShellCupManageState.Waiting;
    }

    public void ShuffleButtle()
    {
        if(state == ShellCupManageState.Filling)
        {
            currentShuffle = 0;
            StartCoroutine(RandomPosition());
        }
    }

    private IEnumerator RandomPosition()
    {
        currentShuffle++;
        int a = Random.Range(0, shellCups.Length);
        int b = Random.Range(0, shellCups.Length);

        while (a == b)
        {
            b = Random.Range(0, shellCups.Length);
        }

        int index = ChangeCurrentIndex(a, b);

        float time = Random.Range(minimalTime, maxTime);

        shellCups[a].GoToIndex(-distance, time, -index);
        shellCups[b].GoToIndex(distance, time, index);

        yield return new WaitUntil(() => shellCups[a].IsIdle() && shellCups[b].IsIdle());
        if (currentShuffle < amountShuffle)
        {
            StartCoroutine(RandomPosition());
        }
        else
        {
            OnSuffleFinished?.Invoke(true);
            state = ShellCupManageState.Guessing;
        }
    }

    private int ChangeCurrentIndex(int first, int second)
    {
        int a = shellCups[first].GetCurrentIndex();
        int b = shellCups[second].GetCurrentIndex();
        shellCups[second].SetCurrentIndex(a);
        shellCups[first].SetCurrentIndex(b);
        return a - b;
    }

    public IEnumerator FillBall(int index)
    {
        if (state == ShellCupManageState.Waiting)
        {
            foreach (var cup in shellCups)
            {
                cup.SetPositionToDefault();
            }
            ballInCup = index;
            Vector3 position = shellCups[index].GetPosition();
            shellBall.GoDown(position, distance);
            yield return new WaitUntil(() => shellBall.IsInPosition());
            state = ShellCupManageState.Filling;
        }
    }

    public IEnumerator CheckBall(int index)
    {
        if (state == ShellCupManageState.Guessing)
        {
            Vector3 position = shellCups[ballInCup].GetPosition();
            ballInCup = shellCups[ballInCup].GetCurrentIndex();
            shellBall.GoUp(position, distance);
            yield return new WaitUntil(() => shellBall.IsInPosition());
            if (index == ballInCup)
            {
                questShellGameManager.UpdateScore(1);
            }
            else
            {
                questShellGameManager.UpdateScore(-1);
            }
            state = ShellCupManageState.Waiting;
        } 
    }
}
