using UnityEngine;

public class SpawnCar : MonoBehaviour
{
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private int amount;
    private CarMovement[] cars;
    private int index = 0;
    [SerializeField] private float maxSummonSpeed;
    [SerializeField] private float minSummonSpeed;
    private float summonSpeed;
    private float time = 0;
    [SerializeField] private Vector3 direction;
    private void Start()
    {
        cars = new CarMovement[amount];
        for (int i = 0; i < amount; i++)
        {
            GameObject car = Instantiate(carPrefab, transform);
            cars[i] = car.GetComponent<CarMovement>();
        }
        summonSpeed = Random.Range(minSummonSpeed, maxSummonSpeed);
    }

    private void Update()
    {
        if(time > summonSpeed)
        {
            summonSpeed = Random.Range(minSummonSpeed, maxSummonSpeed);
            ObjectPooling();
            time = 0;
        }
        else
        {
            time += Time.deltaTime;
        }
    }

    private void ObjectPooling()
    {
        cars[index].SetActive(true);
        cars[index].SetPosition(transform.position);
        cars[index].Direction = direction;
        index = (index + 1) % amount;
    }
}
