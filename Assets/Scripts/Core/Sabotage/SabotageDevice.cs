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

    void Update()
    {
        if (SabotageAction.action.WasPressedThisFrame())
        {
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
    }

}
