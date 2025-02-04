using System.Collections.Generic;
using UnityEngine;

public class DebrisManager : MonoBehaviour
{
    [SerializeField]private float distance = 15f;
    [SerializeField]private int numPoints = 36;
    private List<Vector3> positions = new List<Vector3>();
    [SerializeField] private GameObject debris;
    private int index = 0;
    [SerializeField] private float spawnTime = 2f;
    private float time = 0f;
    private GameObject[] debrises;

    private void Awake()
    {
        GetPositionsAtDistance(transform.position, distance, numPoints);
        debrises = new GameObject[debris.transform.childCount];
        for (int i = 0; i < debris.transform.childCount; i++)
        {
            debrises[i] = debris.transform.GetChild(i).gameObject;
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

    private void GetPositionsAtDistance(Vector3 center, float radius, int numSamples)
    {
        for (int i = 0; i < numSamples; i++)
        {
            float angle = i * (360f / numSamples);
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
        index = (index + 1) % debrises.Length;
    }
}
