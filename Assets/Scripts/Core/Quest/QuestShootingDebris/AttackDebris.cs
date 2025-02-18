using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackDebris : MonoBehaviour
{
    [SerializeField] private InputActionReference clickInputAction;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float cooldown = 2f;
    private float time = 2f;
    private int index = 0;
    private GameObject[] bullets;

    private void Awake()
    {
        bullets = new GameObject[bullet.transform.childCount];  

        for (int i = 0; i < bullet.transform.childCount; i++)
        {
            bullets[i] = bullet.transform.GetChild(i).gameObject;
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
        index = (index + 1) % bullets.Length;
    }
}
