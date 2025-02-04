using System;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class VentInteract : NetworkBehaviour, IInteractable
{
    private ulong interactorID;
    private NetworkObject playerGameObject;
    private bool firstVentEntry = true;
    private int ventID;
    public bool InVent = false;
    private Camera playerCamera;
    public UnityEvent OnInteractVent;

    void Start()
    {
        playerCamera = Camera.main;

        OnInteractVent.AddListener(GetComponentInParent<VentSystem>().InteractVent); //Subscriber
    }


    public void Interact(InteractionData interactionData)
    {

        if (firstVentEntry)
        {
            
            firstVentEntry = false;
            ventID = Array.IndexOf(gameObject.GetComponentInParent<VentSystem>().VentsList, gameObject.transform);
            gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex = ventID;

            if (interactionData.Interactor.TryGetComponent<Player>(out Player interactor))
            {
                if (interactor.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
                {
                    interactorID = networkObject.NetworkObjectId;
                }
            }
        }

        if (interactionData.Interactor.TryGetComponent<Player>(out Player player))
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (!InVent)
            {
                DisablePlayerServerRpc(interactorID); //notify server to disable player

                InVent = true;
                playerCamera.enabled = false;
                gameObject.GetComponentInParent<VentSystem>().VentsList[gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex].GetComponentInChildren<Camera>().enabled = true;
                gameObject.GetComponentInParent<VentSystem>().VentsList[gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex].GetComponentInChildren<VentCamera>().enabled = true;
                OnInteractVent.Invoke();
            }

            else if (InVent)
            {
                cc.enabled = false;
                player.transform.position = gameObject.GetComponentInParent<VentSystem>().VentsList[gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex].position;
                cc.enabled = true;
                EnablePlayerServerRpc(interactorID); //notify server to enable player visibility
                

                InVent = false;
                gameObject.GetComponentInParent<VentSystem>().VentsList[gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex].GetComponentInChildren<Camera>().enabled = false;
                gameObject.GetComponentInParent<VentSystem>().VentsList[gameObject.GetComponentInParent<VentSystem>().CurrentVentIndex].GetComponentInChildren<VentCamera>().enabled = false;
                playerCamera.enabled = true;
                OnInteractVent.Invoke();
                firstVentEntry = true;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisablePlayerServerRpc(ulong playerID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerID, out NetworkObject playerObj))
        {
            playerObj.gameObject.SetActive(false);
            EnablePlayerVisibilityClientRpc(playerID, false); //apply visibility
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void EnablePlayerServerRpc(ulong playerID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerID, out NetworkObject playerObj))
        {
            playerObj.gameObject.SetActive(true);
            EnablePlayerVisibilityClientRpc(playerID, true); //apply visibility
        }
    }

    [ClientRpc]

    private void EnablePlayerVisibilityClientRpc(ulong playerID, bool enable){
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerID, out NetworkObject playerObj))
        {
            playerObj.gameObject.SetActive(enable);
        }
    }

}
