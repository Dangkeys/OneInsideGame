using UnityEngine;

public class CrossGoalController : MonoBehaviour
{
    [SerializeField] private CrossCubeController crossCubeController;
    [SerializeField] private GameObject[] goals;
    [SerializeField] private Material[] materials;
    private Renderer[] renderers;

    private void Awake()
    {
        renderers = new Renderer[goals.Length];
        for (int i = 0; i < goals.Length; i++)
        {
            Renderer render = goals[i].GetComponent<Renderer>();
            renderers[i] = render;
        }
    }

    private void OnEnable()
    {
        ChangeColor();
        crossCubeController.OnReached += OnReachedHandle;
    }

    private void OnDisable()
    {
        crossCubeController.OnReached -= OnReachedHandle;
    }

    private void OnReachedHandle(bool obj)
    {
        if(obj)
        {
            ChangeColor();
        }
    }

    private void ChangeColor()
    {
        if(crossCubeController.Lower)
        {
            renderers[0].material = materials[0];
            renderers[1].material = materials[1];
        }
        else
        {
            renderers[0].material = materials[1];
            renderers[1].material = materials[0];
        }
    }
}
