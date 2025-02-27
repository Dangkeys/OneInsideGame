using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class OxygenSabotage : NetworkBehaviour
{
    // private OxygenInteract oxygenInteract;
    // private void Start()
    // {
    //     oxygenInteract = gameObject.GetComponent<OxygenInteract>();
    // }
    // public void DisableOxygen()
    // {
    //     DisableOxygenServerRpc();
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void DisableOxygenServerRpc()
    // {
    //     //oxygenInteract.Oxygen.SetActive(false);
    //     gameObject.GetComponent<OxygenInteract>().enabled = false;
    //     StartDelayServerRpc();
    // }


    // [ServerRpc(RequireOwnership = false)]
    // private void StartDelayServerRpc()
    // {
    //     //Debug.Log("Start Delay");
    //     StartCoroutine(DisableScript(5));
    // }

    // IEnumerator DisableScript(float delay)
    // {
    //     DisableScriptClientRpc(true);
    //     //Debug.Log("Quest Disabling");
    //     yield return new WaitForSeconds(delay);
    //     //Debug.Log("Quest re-enabled");
    //     DisableScriptClientRpc(false);

    // }

    // [ClientRpc]
    // private void DisableScriptClientRpc(bool status)
    // {
    //     //oxygenInteract.Oxygen.SetActive(status);
    //     gameObject.GetComponent<OxygenInteract>().enabled = status;
    // }



    private OxygenInteract oxygenInteract;

    void Start()
    {
        oxygenInteract = gameObject.GetComponent<OxygenInteract>();
    }


    public void DisableOxygen()
    {
        StartDelayServerRpc();
    }



    [ServerRpc(RequireOwnership = false)]
    private void StartDelayServerRpc()
    {
        //Debug.Log("Start Delay");
        StartCoroutine(DisableScript(5));
    }

    IEnumerator DisableScript(float delay)
    {
            DisableScriptClientRpc(true);
            //Debug.Log("Door Disabling");
            yield return new WaitForSeconds(delay);
            //Debug.Log("Door re-enabled");
            DisableScriptClientRpc(false);

    }

    [ClientRpc]
    private void DisableScriptClientRpc(bool status){
        OxygenInteract script = gameObject.GetComponent<OxygenInteract>();
        script.IsDisabled = status;
    }
}
