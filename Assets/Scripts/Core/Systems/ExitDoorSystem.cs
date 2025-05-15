using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ExitDoorSystem : MonoBehaviour
{
    [SerializeField] private ExitDoor[] doors;
    [SerializeField] private int amountOfDoorsToOpen;
    OneInsideLevelSystem oneInsideLevelSystem;

    private void Awake()
    {
        oneInsideLevelSystem = OneInsideLevelSystem.Instance;
    }

    private void Start()
    {
        oneInsideLevelSystem.IsEndGameCollapse.OnValueChanged += OnIsEndGameCollapseChanged;
    }

    private void OnIsEndGameCollapseChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                List<ExitDoor> shuffledDoors = ShuffleUtility.GetShuffledList(doors);
                
                int doorsToOpen = Mathf.Min(amountOfDoorsToOpen, doors.Length);
                for (int i = 0; i < doorsToOpen; i++)
                {
                    shuffledDoors[i].OpenDoor();
                }
            }
        }
    }
}
