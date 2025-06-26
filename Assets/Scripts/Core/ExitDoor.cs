using UnityEngine;
using Unity.Netcode;

public class ExitDoor : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [field: SerializeField] public NetworkVariable<bool> IsExitGate { get; private set; } = new NetworkVariable<bool>(false);
    public void OpenDoor()
    {
        if (IsServer)
        {
            ChangeColorClientRpc(Color.green);
            IsExitGate.Value = true;
            animator.SetTrigger("Open");
        }
    }
    [ClientRpc]
    private void ChangeColorClientRpc(Color color)
    {
        GetComponent<MeshRenderer>().material.color = color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player player))
        {
            if (IsExitGate.Value && player.Role.Value == PlayerRole.Crewmate && player.IsAlive.Value)
            {
                CrewMateWinServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CrewMateWinServerRpc()
    {
        OneInsideLevelSystem.Instance.SetGameState(GameState.CrewmateWin);
    }
}