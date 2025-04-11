using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class OxygenSabotage : NetworkBehaviour
{
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
        oxygenInteract.IsDisabled = status;
    }
}
