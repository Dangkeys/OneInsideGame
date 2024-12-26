using Unity.VisualScripting;
using UnityEngine;

public class VentTeleport : MonoBehaviour
{
    public Transform warpPosition;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            other.TryGetComponent<CharacterController>(out CharacterController cc);
            cc.enabled = false;
            player.transform.position = warpPosition.position;
            cc.enabled = true;
        }
    }
}
