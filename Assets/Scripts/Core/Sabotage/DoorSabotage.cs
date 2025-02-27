using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DoorSabotage : NetworkBehaviour
{
    InteractionData interactionData;
    private NetworkObject[] doorList;
    private SlideDoor doorScript;

    void Start()
    {
        doorList = gameObject.GetComponentsInChildren<NetworkObject>();
    }


    public void DisableDoor()
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
