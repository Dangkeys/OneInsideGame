using UnityEngine;

public class ChangeTrailColor : MonoBehaviour
{
    [SerializeField] private ShowOffForCube showOffForCube;
    [SerializeField] private Material initMaterial;
    [SerializeField] private Material showMaterial;
    private Renderer render;
    private bool hit = false;
    [SerializeField] private QuestFollowingTrailManager questFollowingTrailManager;

    private void Awake()
    {
        render = GetComponent<Renderer>();
        questFollowingTrailManager.AddAllBlock(1);
    }

    private void OnEnable()
    {
        showOffForCube.OnStart += StartHandle;
        showOffForCube.OnNewGame += NewGameHandle;
    }

    private void OnDisable()
    {
        showOffForCube.OnStart -= StartHandle;
        showOffForCube.OnNewGame -= NewGameHandle;
    }

    private void StartHandle(bool obj)
    {
        if (hit)
            return;
        if (obj)
        {
            render.material = showMaterial;
        }
        else
        {
            render.material = initMaterial;
        }
    }


    private void NewGameHandle(bool obj)
    {
        if(obj)
        {
            hit = false;
            render.material = initMaterial;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Cube") && !hit)
        {
            hit = true;
            questFollowingTrailManager.GetNewBlock(1);
            render.material = showMaterial;
        }
    }
}
