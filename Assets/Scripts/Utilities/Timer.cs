using UnityEngine;
using System;
using UnityEngine.Events;

/// <summary>
/// A utility class for creating and managing timers in Unity.
/// </summary>
public class Timer : MonoBehaviour
{
    /*
    -------------------------------------------------------
    Timer Properties
    -------------------------------------------------------
    */

    /// <summary>
    /// The total duration of the timer in seconds.
    /// </summary>
    private float duration;

    /// <summary>
    /// The current remaining time of the timer in seconds.
    /// </summary>
    private float currentTime;

    /// <summary>
    /// Indicates whether the timer is currently running.
    /// </summary>
    private bool isRunning;

    /// <summary>
    /// An optional callback invoked every frame with the remaining time.
    /// </summary>
    private Action<float> onTick;

    /// <summary>
    /// An optional callback invoked when the timer completes.
    /// </summary>
    private Action onComplete;

    /*
    -------------------------------------------------------
    Static Method
    -------------------------------------------------------
    */

    /// <summary>
    /// Creates a new timer instance.
    /// </summary>
    /// <param name="duration">The duration of the timer in seconds.</param>
    /// <param name="onTick">An optional callback invoked every frame with the remaining time.</param>
    /// <param name="onComplete">An optional callback invoked when the timer completes.</param>
    /// <returns>A new Timer instance.</returns>
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

    /// <summary>
    /// Initializes the timer with the specified duration and callbacks.
    /// </summary>
    /// <param name="duration">The duration of the timer in seconds.</param>
    /// <param name="onTick">An optional callback invoked every frame with the remaining time.</param>
    /// <param name="onComplete">An optional callback invoked when the timer completes.</param>
    private void Initialize(float duration, Action<float> onTick, Action onComplete)
    {
        this.duration = duration;
        this.currentTime = duration;
        this.onTick = onTick;
        this.onComplete = onComplete;
        this.isRunning = true;
    }

    /// <summary>
    /// Updates the timer every frame. Decreases the remaining time and invokes callbacks as needed.
    /// </summary>
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

    /// <summary>
    /// Completes the timer, invokes the onComplete callback, and destroys the timer GameObject.
    /// </summary>
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

    /// <summary>
    /// Cancels the timer and destroys the timer GameObject.
    /// </summary>
    public void Cancel()
    {
        isRunning = false;
        Destroy(gameObject);
    }

    /// <summary>
    /// Pauses the timer, stopping it from counting down.
    /// </summary>
    public void Pause()
    {
        isRunning = false;
    }

    /// <summary>
    /// Resumes the timer, allowing it to continue counting down.
    /// </summary>
    public void Resume()
    {
        isRunning = true;
    }

    /// <summary>
    /// Gets the remaining time of the timer in seconds.
    /// </summary>
    /// <returns>The remaining time in seconds.</returns>
    public float GetTimeLeft()
    {
        return currentTime;
    }
}
