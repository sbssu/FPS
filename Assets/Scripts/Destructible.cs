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

        // 충돌한 물체의 rigidbody에 접근해 질량 * 속력(F=MA)으로 힘을 구한다.
        float mass = collision.rigidbody.mass;
        float force = mass * accel;
        health = Mathf.Clamp(health - force, 0f, float.MaxValue);

        if (health <= 0)
            Crush();
    }

}
