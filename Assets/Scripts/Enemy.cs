using System.Collections;
using System.Collections.Generic;
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

    public void Hit(float damage)
    {
        hp = Mathf.Clamp(hp - damage, 0f, maxHp);
        if (hp > 0)
            return;

        Destroy(gameObject);
    }
}
