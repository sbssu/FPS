using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] GameObject destructObject;
    [SerializeField] float health;

    public void Crush()
    {
        Instantiate(destructObject, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null)
            return;

        float accel = collision.rigidbody.velocity.magnitude;
        if (accel < 1)
            return;

        // �浹�� ��ü�� rigidbody�� ������ ���� * �ӷ�(F=MA)���� ���� ���Ѵ�.
        float mass = collision.rigidbody.mass;
        float force = mass * accel;
        health = Mathf.Clamp(health - force, 0f, float.MaxValue);

        if (health <= 0)
            Crush();
    }

}
