using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class SabotageDevice : NetworkBehaviour
{
    public InputActionReference SabotageAction;
    private bool isSabotaging;
    public GameObject SabotageUI;
    [field: SerializeField, Tooltip("Reference to the input system")]
    public InputReader InputReader { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        InputReader.OpenSabotageUIEvent += OpenSabotageUI;
    }

    private void OpenSabotageUI(){
        Debug.Log("Sabotage button pressed");
            if (gameObject.GetComponent<Player>().Role.Value == PlayerRole.Imposter)
            {
                if (!isSabotaging)
                {
                    isSabotaging = true;
                    SabotageUI.SetActive(true);
                }
                else
                {
                    isSabotaging = false;
                    SabotageUI.SetActive(false);
                }
            }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;
        InputReader.OpenSabotageUIEvent -= OpenSabotageUI;
    }

}
