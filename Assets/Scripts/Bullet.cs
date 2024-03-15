using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float TimeToDestroyed;

    float moveSpeed;
    float damage;
    float showTime;

    public void Shoot(float moveSpeed, float damage)
    {
        this.moveSpeed = moveSpeed;
        this.damage = damage;
        showTime = 0.0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
        if((showTime += Time.deltaTime) >= TimeToDestroyed)
        {
            Destroy(gameObject);
        }
    }
}
