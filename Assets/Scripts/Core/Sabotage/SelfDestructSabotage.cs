using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelfDestructSabotage : NetworkBehaviour
{
    [SerializeField] private int delay;
    [SerializeField] private TextMeshProUGUI timer;

    public void StartCountdown()
    {
        StartCountdownServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartCountdownServerRpc()
    {
        timer.enabled = true;
        StartCoroutine(Countdown(delay));
    }

    IEnumerator Countdown(float delay)
    {
        float timeleft = delay;
        while (timeleft > 0)
        {
            SetTimerTextServerRpc(timeleft);
            Debug.Log(timeleft);
            yield return new WaitForSeconds(1);
            timeleft--;
        }
        timer.enabled = false;
    }

    [ServerRpc]
    private void SetTimerTextServerRpc(float delay)
    {
        timer.text = delay.ToString();
    }
}
