using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float maxHp;
    [SerializeField] SkinnedMeshRenderer meshRender;

    float hp;
    private void Start()
    {
        hp = maxHp;
    }

    public void OnHit(HITTYPE type, float damage)
    {
        damage = Hitbox.CalculateDamage(type, damage);
        Debug.Log($"{name} HIT {type} : {damage}");
        hp = Mathf.Clamp(hp - damage, 0f, maxHp);
        if (hp > 0)
            return;

        Destroy(gameObject);
    }
}
