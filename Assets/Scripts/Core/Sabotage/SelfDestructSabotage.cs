using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelfDestructSabotage : NetworkBehaviour
{
    [SerializeField] private int delay;
    [SerializeField] private GameObject timer;

    public void StartCountdown()
    {
        StartCountdownServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartCountdownServerRpc()
    {
        EnableUIClientRpc(true);
        StartCoroutine(Countdown(delay));
    }

    IEnumerator Countdown(float delay)
    {
        float timeleft = delay;
        while (timeleft > 0)
        {
            SetTimerTextClientRpc(timeleft);
            Debug.Log(timeleft);
            yield return new WaitForSeconds(1);
            timeleft--;
        }
        EnableUIClientRpc(false);
    }

    [ClientRpc]
    private void SetTimerTextClientRpc(float delay)
    {
        timer.GetComponent<TextMeshProUGUI>().text = delay.ToString();
    }


    [ClientRpc]
    private void EnableUIClientRpc(bool status){
        timer.SetActive(status);
    }
}
