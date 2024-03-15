using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    enum STATE
    {
        TAKEOUT,    // ����̱�
        IDLE,       // ������
        FIRE,       // ���
        REROAD,     // ������
    }
    enum TRIGGER
    {
        AUTO,       // �ڵ�(����)
        MANUAL,     // ����(�ܹ�)
    }

    [Header("State")]
    [SerializeField] STATE state;
    [SerializeField] TRIGGER trigger;
    [SerializeField] bool isAutoReload;

    [Header("Object")]
    [SerializeField] Bullet bulletPrefab;   // �Ѿ� ������.
    [SerializeField] Transform muzzle;      // �ѱ�.

    [Header("Animator Clip")]
    [SerializeField] AnimationClip takeOutClip;
    [SerializeField] AnimationClip fireClip;
    [SerializeField] AnimationClip reloadClip;

    [Header("Parametor")]
    [SerializeField] float fireRate;        // ����ӵ�.
    [SerializeField] float speed;           // ź��.
    [SerializeField] float damage;          // ������.
    [SerializeField] int maxAmmo;           // �ִ� ź���.

    bool isTriggerDown;
    bool isShotDone;
    public bool IsTriggerDown
    {
        set
        {
            isTriggerDown = value;
            if (!isTriggerDown)         // relaease mouse
                isShotDone = false;
        }
    }

    int currentAmmo;            // ���� ź���.
    float rateTimer;
    Animator anim;
    PlayerController owner;

    public void Setup(PlayerController owner)
    {
        this.owner = owner;
        anim = GetComponent<Animator>();

        fireHash = Animator.StringToHash(fireClip.name);
        reloadHash = Animator.StringToHash(reloadClip.name);
        takeOutHash = Animator.StringToHash(takeOutClip.name);

        currentAmmo = maxAmmo;
    }
    public void Pickup()
    {
        anim.SetFloat("fireSpeed", fireClip.length / fireRate);     // ����ӵ��� ���� �ִϸ��̼� ���ǵ� ����.
        gameObject.SetActive(true);
    }
    public void PutAway()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateWeaponState();

        // Ʈ���Ű� ������ �ѱ⸦ �߻��Ѵ�.
        if (state != STATE.TAKEOUT && isTriggerDown)
        {
            if (trigger == TRIGGER.MANUAL && !isShotDone)
            {
                isShotDone = true;
                Fire();
            }
            else if (trigger == TRIGGER.AUTO)
                Fire();
        }

        // ���� �� ����.
        rateTimer = Mathf.Clamp(rateTimer - Time.deltaTime, 0.0f, fireRate);

        // �Ķ���� ����.
        anim.SetFloat("movement", owner.currentSpeed);
    }

    int takeOutHash;
    int fireHash;
    int reloadHash;
    private void UpdateWeaponState()
    {
        STATE wasState = state;

        // ���� ������� �ִϸ��̼� Ŭ���� hash�� ���� �ѱ��� ���� ����.
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.shortNameHash == fireHash)
            state = STATE.FIRE;
        else if (info.shortNameHash == reloadHash)
            state = STATE.REROAD;
        else if (info.shortNameHash == takeOutHash)
            state = STATE.TAKEOUT;
        else
            state = STATE.IDLE;

        // ���� ��ȭ�� ���� ó��.
        if (wasState != state)
        {
            // �ٷ� ���� ���°� Reload����. (=�������� ������)
            switch (wasState)
            {
                case STATE.REROAD:
                    currentAmmo = maxAmmo;
                    break;
                case STATE.FIRE:
                    if (isAutoReload && currentAmmo <= 0)
                        Reload(true);
                    break;
            }
        }
    }

    private void Fire()
    {
        if (rateTimer <= 0.0f && currentAmmo > 0 && state == STATE.IDLE)
        {
            Bullet bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
            bullet.Shoot(speed, damage);

            anim.SetTrigger("onFire");
            rateTimer = fireRate;
            currentAmmo -= 1;
        }
    }
    public void Reload(bool isForce = false)
    {
        if (state == STATE.IDLE || isForce)
        {
            anim.SetTrigger("onReload");
        }
    }

    private void PlayFootstep()
    {

    }
}
