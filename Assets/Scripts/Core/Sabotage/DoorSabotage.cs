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
        DisableDoorServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisableDoorServerRpc()
    {
        for (int i = 0; i < doorList.Length; i++)
        {
            doorScript = doorList[i].GetComponentInChildren<SlideDoor>();
            if (doorScript.DoorOpen)
            {
                doorScript.Interact(interactionData);
            }
            StartDelayServerRpc(doorList[i].GetComponent<NetworkObject>().NetworkObjectId);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void StartDelayServerRpc(ulong doorID)
    {
        //Debug.Log("Start Delay");
        StartCoroutine(DisableScript(5, doorID));
    }

    IEnumerator DisableScript(float delay, ulong doorID)
    {
            DisableScriptClientRpc(doorID, true);
            //Debug.Log("Door Disabling");
            yield return new WaitForSeconds(delay);
            //Debug.Log("Door re-enabled");
            DisableScriptClientRpc(doorID, false);

    }

    [ClientRpc]
    private void DisableScriptClientRpc(ulong doorID, bool status){
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(doorID, out NetworkObject netObj);
        SlideDoor script = netObj.GetComponentInChildren<SlideDoor>();
        script.IsDisabled = status;
    }




}
