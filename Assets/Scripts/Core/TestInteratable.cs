using UnityEngine;

public class TestInteratable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interact");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
