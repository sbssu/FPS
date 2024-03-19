using System;
using UnityEngine;

public class Bullet : MonoBehaviour, IReturnPool<Bullet>
{
    [SerializeField] protected float TimeToDestroyed;
    [SerializeField] protected ParticleSystem hitVfx;

    protected float moveSpeed;
    protected float damage;
    protected float showTime;

    public event Action<Bullet> release;

    public virtual void Shoot(float moveSpeed, float damage)
    {
        this.moveSpeed = moveSpeed;
        this.damage = damage;
        showTime = 0.0f;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
            enemy.Hit(damage);

        Vector3 normal = collision.contacts[0].normal;

        // 히트 이펙트 재생.
        ParticleSystem vfx = Instantiate(hitVfx);
        vfx.transform.position = transform.position;
        vfx.transform.rotation = Quaternion.LookRotation(normal);

        Release();
    }

    protected virtual void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
        if((showTime += Time.deltaTime) >= TimeToDestroyed)
        {
            Release();
        }
    }

    protected void Release()
    {
        release?.Invoke(this);
    }
}
