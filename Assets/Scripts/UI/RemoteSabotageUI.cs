using System;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;

public class RemoteSabotageUI : MonoBehaviour
{
    private Button doorButton;

    void Start()
    {
        doorButton = GetComponentInChildren<Button>();
        doorButton.onClick.AddListener(DisableDoor);
    }

    private void DisableDoor()
    {
        DoorSabotage doorSabotage = FindAnyObjectByType<DoorSabotage>();
        Debug.Log($"Found door sabotage: {doorSabotage}");
        if (doorSabotage != null)
        {
            Debug.Log("Disabling door");
            doorSabotage.DisableDoor();
        }
    }
}
