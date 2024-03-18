using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float TimeToDestroyed;
    [SerializeField] ParticleSystem hitVfx;

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
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
            enemy.Hit(damage);

        Vector3 normal = collision.contacts[0].normal;

        // 히트 이펙트 재생.
        ParticleSystem vfx = Instantiate(hitVfx);
        vfx.transform.position = transform.position;
        vfx.transform.rotation = Quaternion.LookRotation(normal);

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
