using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    Animator anim;
    PlayerController owner;

    public void Setup(PlayerController owner)
    {
        this.owner = owner;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        anim.SetFloat("movement", owner.currentSpeed);
    }

    private void PlayFootstep()
    {

    }
}
