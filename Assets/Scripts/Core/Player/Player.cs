using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [field: SerializeField] public CinemachineCamera VirtualCamera {get; private set;}
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            VirtualCamera.Priority = int.MinValue;
        }
    }


    private void Update()
    {
        if (!IsOwner) return;

    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

    }
}
