using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public PlayerMovement PlayerMovement { get; private set; }
    [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
    [field: SerializeField] public CinemachineInputAxisController AxisController { get; private set; }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            VirtualCamera.Priority = int.MinValue;
        }
        else
        {
            if (!OneInsideLevelManager.Instance)
                return;
            OneInsideLevelManager.Instance.PlayerManager.OnSetAllPlayersToSpawnPos += ResetToSpawnPoint;
            OneInsideLevelManager.Instance.PlayerManager.OnEnableAllPlayersMovement += EnablePlayerMovement;
        }
    }

    private void EnablePlayerMovement(bool shouldMove)
    {
        if (!shouldMove)
        {
            InputReader.DisableGameplayInput();

        }
        else
        {
            InputReader.EnableGameplayInput();
        }
        if(AxisController)
            AxisController.enabled = shouldMove;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;
        if (!OneInsideLevelManager.Instance)
            return;
        OneInsideLevelManager.Instance.PlayerManager.OnSetAllPlayersToSpawnPos -= ResetToSpawnPoint;
    }
    public void ResetToSpawnPoint()
    {
        CharacterController.enabled = false;
        transform.position = SpawnPoint.GetClientSpawnPos(NetworkManager.Singleton.LocalClientId);
        CharacterController.enabled = true;
    }
}
