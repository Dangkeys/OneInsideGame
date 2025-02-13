using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    [SerializeField] private GameObject[] cameras;

    public void SwitchCamera(int index, int newIndex)
    {
        if (cameras[index].activeInHierarchy)
        {
            cameras[index].SetActive(false);
            cameras[newIndex].SetActive(true);
        }
    }
}
