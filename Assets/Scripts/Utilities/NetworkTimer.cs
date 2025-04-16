using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Provides a networked timer that can be started, stopped, paused, and resumed across clients.
/// All timer state is synchronized using NetworkVariables.
/// </summary>
public class NetworkTimer : NetworkBehaviour
{
    /// <summary>
    /// Network synchronized time remaining on the timer in seconds.
    /// </summary>
    public NetworkVariable<float> TimeRemaining { get; private set; } = new NetworkVariable<float>(0f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    
    /// <summary>
    /// Network synchronized flag indicating if the timer is currently active.
    /// </summary>
    public NetworkVariable<bool> IsTimerActive { get; private set; } = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
        
    /// <summary>
    /// Callback that will be invoked when the timer completes.
    /// </summary>
    private Action onTimerCompleteCallback;
    
    /// <summary>
    /// Reference to the active timer coroutine.
    /// </summary>
    private Coroutine timerCoroutine;
    
    /// <summary>
    /// Returns whether the timer is currently running.
    /// </summary>
    public bool IsTimerRunning => IsTimerActive.Value;
    
    /// <summary>
    /// Returns the current time remaining on the timer.
    /// </summary>
    public float CurrentTime => TimeRemaining.Value;
    
    /// <summary>
    /// Called when the network object spawns. Sets up event listeners.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            IsTimerActive.OnValueChanged += OnTimerActiveChanged;
        }
        
        TimeRemaining.OnValueChanged += OnTimeRemainingChanged;
    }
    
    /// <summary>
    /// Called when the network object despawns. Cleans up event listeners and coroutines.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            IsTimerActive.OnValueChanged -= OnTimerActiveChanged;
            
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }
        
        TimeRemaining.OnValueChanged -= OnTimeRemainingChanged;
        
        base.OnNetworkDespawn();
    }
    
    /// <summary>
    /// Starts the timer with the specified duration and optional completion callback.
    /// Works on both server and client.
    /// </summary>
    /// <param name="duration">Duration of the timer in seconds</param>
    /// <param name="onComplete">Optional callback to invoke when timer completes</param>
    public void StartTimer(float duration, Action onComplete = null)
    {
        onTimerCompleteCallback = onComplete;
        
        if (IsServer)
        {
            StartTimerOnServer(duration);
        }
        else
        {
            StartTimerServerRpc(duration);
        }
    }
    
    /// <summary>
    /// Server-side implementation of starting the timer.
    /// </summary>
    /// <param name="duration">Duration of the timer in seconds</param>
    private void StartTimerOnServer(float duration)
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        
        TimeRemaining.Value = duration;
        
        IsTimerActive.Value = true;
    }
    
    /// <summary>
    /// ServerRpc to start the timer from a client.
    /// </summary>
    /// <param name="duration">Duration of the timer in seconds</param>
    [ServerRpc(RequireOwnership = false)]
    public void StartTimerServerRpc(float duration)
    {
        StartTimerOnServer(duration);
    }
    
    /// <summary>
    /// Stops the timer completely. Works on both server and client.
    /// </summary>
    public void StopTimer()
    {
        if (IsServer)
        {
            IsTimerActive.Value = false;
        }
        else
        {
            StopTimerServerRpc();
        }
    }
    
    /// <summary>
    /// ServerRpc to stop the timer from a client.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StopTimerServerRpc()
    {
        IsTimerActive.Value = false;
    }
    
    /// <summary>
    /// Pauses the timer without resetting it. Works on both server and client.
    /// </summary>
    public void PauseTimer()
    {
        if (IsServer)
        {
            IsTimerActive.Value = false;
        }
        else
        {
            PauseTimerServerRpc();
        }
    }
    
    /// <summary>
    /// ServerRpc to pause the timer from a client.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void PauseTimerServerRpc()
    {
        IsTimerActive.Value = false;
    }
    
    /// <summary>
    /// Resumes a paused timer. Works on both server and client.
    /// </summary>
    public void ResumeTimer()
    {
        if (IsServer)
        {
            IsTimerActive.Value = true;
        }
        else
        {
            ResumeTimerServerRpc();
        }
    }
    
    /// <summary>
    /// ServerRpc to resume the timer from a client.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ResumeTimerServerRpc()
    {
        IsTimerActive.Value = true;
    }
    
    /// <summary>
    /// Handles changes to the timer's active state.
    /// Starts or stops the timer coroutine as needed.
    /// </summary>
    /// <param name="previousValue">Previous active state</param>
    /// <param name="newValue">New active state</param>
    private void OnTimerActiveChanged(bool previousValue, bool newValue)
    {
        if (!IsServer) return;
        
        if (newValue == true && previousValue == false)
        {
            timerCoroutine = StartCoroutine(TimerCoroutine());
        }
        else if (newValue == false && previousValue == true)
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }
    }
    
    /// <summary>
    /// Handles changes to the timer's remaining time.
    /// Invokes the completion callback when the timer reaches zero.
    /// </summary>
    /// <param name="previousValue">Previous time value</param>
    /// <param name="newValue">New time value</param>
    private void OnTimeRemainingChanged(float previousValue, float newValue)
    {
        if (previousValue > 0 && newValue <= 0)
        {
            onTimerCompleteCallback?.Invoke();
        }
    }
    
    /// <summary>
    /// Coroutine that decrements the timer at regular intervals.
    /// Runs only on the server.
    /// </summary>
    private IEnumerator TimerCoroutine()
    {
        while (TimeRemaining.Value > 0f && IsTimerActive.Value)
        {
            yield return new WaitForSeconds(0.01f);
            
            TimeRemaining.Value -= 0.01f;
            
            if (TimeRemaining.Value <= 0f)
            {
                TimeRemaining.Value = 0f;
                IsTimerActive.Value = false;
                break;
            }
        }
    }
}