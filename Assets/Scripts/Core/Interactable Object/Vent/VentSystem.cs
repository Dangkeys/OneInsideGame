using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class VentSystem : NetworkBehaviour
{
    public int CurrentVentIndex;
    public Transform[] VentsList;
    private bool playerVentStatus;
    public InputActionReference PreviousAction;

    public InputActionReference ForwardAction;
    void Start()
    {
        VentsList = transform.GetComponentsInChildren<Transform>();
        VentsList = transform.Cast<Transform>().Where(t => t != transform).ToArray(); //Exclude the parent transform
    }

    void Update()
    {
        if (playerVentStatus) //if player is inside vent
        {
            if (PreviousAction.action.WasPressedThisFrame())
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
            if (ForwardAction.action.WasPressedThisFrame())
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
