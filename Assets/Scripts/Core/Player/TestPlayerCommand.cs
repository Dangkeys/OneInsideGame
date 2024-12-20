using QFSW.QC;
using UnityEngine;

public class TestPlayerCommand : MonoBehaviour
{
    [Command]
    private void Teleport() {
        gameObject.transform.position = new Vector3(20,20,20);
    }
}
