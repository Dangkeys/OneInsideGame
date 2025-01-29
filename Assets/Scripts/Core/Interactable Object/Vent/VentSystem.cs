using UnityEngine;

public class VentSystem : MonoBehaviour
{
    private GameObject[] ventsList;

    void Start()
    {
        ventsList = GetComponentsInChildren<GameObject>();
    }

    public void AccessVent()
    {

    }
}
