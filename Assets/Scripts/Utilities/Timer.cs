using UnityEngine;
using System;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    /*
    -------------------------------------------------------
    Timer Properties
    -------------------------------------------------------
    */
    private float duration;
    private float currentTime;
    private bool isRunning;
    private Action<float> onTick;
    private Action onComplete;

    /*
    -------------------------------------------------------
    Static Method
    -------------------------------------------------------
    */
    public static Timer Create(float duration, Action<float> onTick = null, Action onComplete = null)
    {
        GameObject go = new GameObject("Timer");
        Timer timer = go.AddComponent<Timer>();
        timer.Initialize(duration, onTick, onComplete);
        return timer;
    }

    /*
    -------------------------------------------------------
    Main
    -------------------------------------------------------
    */
    private void Initialize(float duration, Action<float> onTick, Action onComplete)
    {
        this.duration = duration;
        this.currentTime = duration;
        this.onTick = onTick;
        this.onComplete = onComplete;
        this.isRunning = true;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        currentTime -= Time.deltaTime;
        onTick?.Invoke(currentTime);

        if (currentTime <= 0)
        {
            Complete();
        }
    }

    private void Complete()
    {
        isRunning = false;
        onComplete?.Invoke();
        Destroy(gameObject);
    }

    /*
    -------------------------------------------------------
    Public Methods
    -------------------------------------------------------
    */
    public void Cancel()
    {
        isRunning = false;
        Destroy(gameObject);
    }

    public void Pause()
    {
        isRunning = false;
    }

    public void Resume()
    {
        isRunning = true;
    }

    public float GetTimeLeft()
    {
        return currentTime;
    }
}
