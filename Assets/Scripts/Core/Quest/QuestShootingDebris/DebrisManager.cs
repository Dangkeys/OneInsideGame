using System.Collections.Generic;
using UnityEngine;

public class DebrisManager : MonoBehaviour
{
    [SerializeField]private float distance = 15f;
    private List<Vector3> positions = new List<Vector3>();
    private int index = 0;
    [SerializeField] private float spawnTime = 2f;
    private float time = 0f;
    private List<GameObject> debrises = new List<GameObject>();
    [SerializeField] private GameObject debrisePrefab;
    [SerializeField] private Transform debriseLocation;
    [SerializeField] private int debriseAmount = 10;

    private void Awake()
    {
        GetPositionsAtDistance(transform.position, distance);
    }

    private void Start()
    {
        ObjectPooling();
    }

    private void ObjectPooling()
    {
        for (int i = 0; i < debriseAmount; i++)
        {
            GameObject obj = Instantiate(debrisePrefab, debriseLocation);
            debrises.Add(obj);
        }
    }
    private void OnDisable()
    {
        SetDebrisInactive();
    }

    private void Update()
    {
        if (time < spawnTime)
        {
            time += Time.deltaTime;
        }
        else
        {
            SpawnDebris();
            time = 0f;
        }
    }

    private void SetDebrisInactive()
    {
        foreach (var debris in debrises)
        {
            debris.SetActive(false);
        }
    }

    private void GetPositionsAtDistance(Vector3 center, float radius)
    {
        for (int i = 0; i < 36; i++)
        {
            float angle = i * 100f;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
            positions.Add(center + offset);
        }
    }

    private int RandomPositionIndex()
    {
        return Random.Range(0, positions.Count);
    }

    private void SpawnDebris()
    {
        int positionIndex = RandomPositionIndex();
        debrises[index].SetActive(true);
        debrises[index].GetComponent<DebrisMove>().Setinit((transform.position - positions[positionIndex]).normalized, positions[positionIndex], transform.position);
        index = (index + 1) % debrises.Count;
    }
}
