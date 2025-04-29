using UnityEngine;

public class BlockPattern : MonoBehaviour
{
    [SerializeField] private char pattern;
    [SerializeField] private RandomPattern randomPattern;
    [SerializeField] private Material initMaterial;
    [SerializeField] private Material activeMaterial;
    private Renderer render;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;

    private void Awake()
    {
        randomPattern.AddCharacter(pattern);
        render = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        render.material = initMaterial;
        randomPattern.OnCharacterUsed += CheckCharacter;
    }

    private void OnDisable()
    {
        randomPattern.OnCharacterUsed -= CheckCharacter;
    }

    private void CheckCharacter(char ch)
    {
        if (ch == pattern)
        {
            render.material = activeMaterial;
        }
        else if (ch == '+')
        {
            render.material = winMaterial;
        }
        else if (ch == '-')
        {
            render.material = loseMaterial;
        }
        else
        {
            render.material = initMaterial;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Cube") && randomPattern.CanSent())
        {
            randomPattern.AddCheck(pattern);
            render.material = activeMaterial;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Cube") && randomPattern.CanSent())
        {
            render.material = initMaterial;
        }
    }
}
