using UnityEngine;
using Unity.Netcode;
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
                Debug.Log("OnIsEndGameCollapseChanged");
                for (int i = 0; i < amountOfDoorsToOpen; i++)
                {
                    doors[Random.Range(0, doors.Length)].OpenDoor();
                }
            }

        }
    }


}
