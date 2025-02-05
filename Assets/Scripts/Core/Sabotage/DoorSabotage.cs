using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class DoorSabotage : MonoBehaviour
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
        for (int i = 0; i < doorList.Length; i++)
        {
            doorScript = doorList[i].GetComponentInChildren<SlideDoor>();
            if (doorScript.DoorOpen)
            {
                doorScript.Interact(interactionData);
            }
            StartCoroutine(DisableScript(5, doorScript));
        }
    }

    IEnumerator DisableScript(float delay, SlideDoor script)
    {
        script.IsDisabled = true;
        Debug.Log("Waiting");
        yield return new WaitForSeconds(5f);
        Debug.Log("Finished");
        script.IsDisabled = false;
    }
}
