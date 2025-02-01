using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class VentSystem : NetworkBehaviour
{
    public int CurrentVentIndex;
    public Transform[] VentsList;
    private bool playerVentStatus;

    void Start()
    {
        VentsList = transform.GetComponentsInChildren<Transform>();
        VentsList = transform.Cast<Transform>().Where(t => t != transform).ToArray(); //Exclude the parent transform
    }

    void Update()
    {
        if (playerVentStatus) //if player is inside vent
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                VentsList[CurrentVentIndex].GetComponentInChildren<Camera>().enabled = false;
                VentsList[CurrentVentIndex].GetComponentInChildren<VentCamera>().enabled = false;
                CurrentVentIndex--;
                if (CurrentVentIndex < 0)
                {
                    CurrentVentIndex = VentsList.Length - 1;
                }
                VentsList[CurrentVentIndex].GetComponentInChildren<Camera>().enabled = true;
                VentsList[CurrentVentIndex].GetComponentInChildren<VentCamera>().enabled = true;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                VentsList[CurrentVentIndex].GetComponentInChildren<Camera>().enabled = false;
                VentsList[CurrentVentIndex].GetComponentInChildren<VentCamera>().enabled = false;
                CurrentVentIndex++;
                if (CurrentVentIndex > VentsList.Length - 1)
                {
                    CurrentVentIndex = 0;
                }
                VentsList[CurrentVentIndex].GetComponentInChildren<Camera>().enabled = true;
                VentsList[CurrentVentIndex].GetComponentInChildren<VentCamera>().enabled = true;
            }
        }
    }

    public void InteractVent()
    {
        if (CurrentVentIndex >= 0 && CurrentVentIndex < VentsList.Length)    
        {
            playerVentStatus = VentsList[CurrentVentIndex].GetComponent<VentInteract>().InVent;   //Update if player is inside vent or outside
        }
    }


}
