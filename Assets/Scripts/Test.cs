using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    [field: SerializeField]
    public InputReader InputReader { get; private set; }
    private void Awake()
    {
        InputReader.Initialize();
    }

    private void Start()
    {
        InputReader.MoveEvent += HandleMove;
    }

    private void HandleMove(Vector2 vector)
    {
        Debug.Log(vector);
    }
}
