using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackDebris : MonoBehaviour
{
    [SerializeField] private InputActionReference clickInputAction;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float cooldown = 2f;
    private float time = 2f;
    private int index = 0;
    private List<GameObject> bullets = new List<GameObject>();
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletLocation;
    [SerializeField] private int bulletAmount = 10;

    private void Start()
    {
        ObjectPooling();
    }

    private void ObjectPooling()
    {
        for(int i = 0;i < bulletAmount; i++)
        {
            GameObject obj = Instantiate(bulletPrefab, bulletLocation);
            bullets.Add(obj);
        }
    }


    private void OnEnable()
    {
        time = cooldown;
        index = 0;
        clickInputAction.action.performed += HandleClick;
    }

    private void OnDisable()
    {
        SetBulletInActive();
        clickInputAction.action.performed -= HandleClick;
    }

    private void Update()
    {
        if (time < cooldown)
        {
            time += Time.deltaTime;
        }
    }

    private void HandleClick(InputAction.CallbackContext context)
    {
        if (time >= cooldown)
        {
            Attack();
            time = 0f;
        }
    }

    private void SetBulletInActive()
    {
        foreach (var bullet in bullets)
        {
            bullet.SetActive(false);
        }
    }
    private void Attack()
    {
        bullets[index].SetActive(true);
        bullets[index].GetComponent<BulletMove>().Setinit(transform.forward, transform.position);
        index = (index + 1) % bullets.Count;
    }
}
