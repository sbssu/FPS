using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

public class Gun : MonoBehaviour
{
    protected enum STATE
    {
        TAKEOUT,    // ����̱�
        IDLE,       // ������
        FIRE,       // ���
        REROAD,     // ������
    }
    protected enum TRIGGER
    {
        AUTO,       // �ڵ�(����)
        MANUAL,     // ����(�ܹ�)
    }

    [Header("State")]
    [SerializeField] protected STATE state;
    [SerializeField] protected TRIGGER trigger;

    [Ammo]
    [SerializeField] protected int ammoType;
    [SerializeField] protected bool isAutoReload;

    [Header("Object")]
    [SerializeField] protected Bullet bulletPrefab;   // �Ѿ� ������.
    [SerializeField] protected Transform muzzle;      // �ѱ�.

    [Header("Animator Clip")]
    [SerializeField] protected AnimationClip takeOutClip;
    [SerializeField] protected AnimationClip fireClip;
    [SerializeField] protected AnimationClip reloadClip;

    [Header("Parametor")]
    [SerializeField] protected float fireRate;        // ����ӵ�.
    [SerializeField] protected float speed;           // ź��.
    [SerializeField] protected float damage;          // ������.
    [SerializeField] protected int maxAmmo;           // �ִ� ź���.

    protected const float MAX_DISTANCE = 100f;

    protected bool isTriggerDown;
    protected bool isShotDone;
    public bool IsTriggerDown
    {
        set
        {
            isTriggerDown = value;
            if (!isTriggerDown)         // relaease mouse
                isShotDone = false;
        }
    }

    protected int currentAmmo;            // ���� ź���.
    protected float rateTimer;
    protected Animator anim;
    protected PlayerController owner;
    protected ObjectPool<Bullet> pool;

    public void Setup(PlayerController owner)
    {
        this.owner = owner;
        anim = GetComponent<Animator>();

        fireHash = Animator.StringToHash(fireClip.name);
        reloadHash = Animator.StringToHash(reloadClip.name);
        takeOutHash = Animator.StringToHash(takeOutClip.name);

        // ���ʿ� Ǯ���� �̿��� �Ѿ��� ���� �����Ѵ�.
        pool = new ObjectPool<Bullet>();
        pool.Setup(gameObject, bulletPrefab, 30);
    }
    public void Pickup()
    {
        if(currentAmmo <= 0)
            currentAmmo += owner.GetAmmo(ammoType, maxAmmo);

        gameObject.SetActive(true);

        anim.SetFloat("fireSpeed", fireClip.length / fireRate);     // ����ӵ��� ���� �ִϸ��̼� ���ǵ� ����.
        UIManager.Instance.UpdateGunName(gameObject.name);
        UIManager.Instance.UpdateAmmo(currentAmmo, owner.GetMaxAmmo(ammoType));
    }
    public void PutAway()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateWeaponState();
        UpdateRayTarget();
        UpdateWeaponControl();

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
                    currentAmmo += owner.GetAmmo(ammoType, maxAmmo - currentAmmo);
                    UIManager.Instance.UpdateAmmo(currentAmmo, owner.GetMaxAmmo(ammoType));
                    break;
                case STATE.FIRE:
                    if (isAutoReload && currentAmmo <= 0)
                        Reload(true);
                    break;
            }
        }
    }
    private void UpdateRayTarget()
    {
        // ���� ��ǥ ���� ����.
        Transform camPivot = Camera.main.transform;
        hitPoint = Vector3.zero;
        if (Physics.Raycast(camPivot.position, camPivot.forward, out RaycastHit hit, MAX_DISTANCE))
            hitPoint = hit.point;
        else
            hitPoint = camPivot.position + camPivot.forward * MAX_DISTANCE;
    }
    protected virtual void UpdateWeaponControl()
    {
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
    }

    protected Vector3 hitPoint;
    private void Fire()
    {
        if (rateTimer <= 0.0f && currentAmmo > 0 && state == STATE.IDLE)
        {
            Bullet bullet = pool.Get();                         // Ǯ������ �ϳ��� ������.
            bullet.transform.position = muzzle.position;        // �ѱ� ��ġ�� �̵�.
            bullet.transform.LookAt(hitPoint);                  // �ش� �������� �ٶ󺸴� ȸ��
            bullet.Shoot(speed, damage);                        // �߻� �Լ� ȣ��.

            anim.SetTrigger("onFire");
            rateTimer = fireRate;
            currentAmmo -= 1;
            UIManager.Instance.UpdateAmmo(currentAmmo, owner.GetMaxAmmo(ammoType));
        }
    }
    public virtual void Reload(bool isForce = false)
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


// Ammo��� �̸��� Attribute(�Ӽ�)�� ����
public class AmmoAttribute : PropertyAttribute
{

}

// AmmoAttribute ������Ƽ�� ������� GUI�� Ŀ���͸���¡�ϴ� Ŭ����
[CustomPropertyDrawer(typeof(AmmoAttribute))]
public class AmmoDrawer : PropertyDrawer
{
    // position : inspector���� ��ġ ��
    // property : ��� ����Ƽ property�� ã�� ���� �� �ִ� �ֻ��� ��ü
    // label : gui���� �ɼ� ��
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AmmoData db = Database.Instance.ammoData;
        if (db.entries == null || db.entries.Length == 0)
        {
            EditorGUI.HelpBox(position, "DB�� ź�� ������ ���Ե��� �ʾҽ��ϴ�.", MessageType.Warning);
        }
        else
        {
            int value = property.intValue;
            int index = System.Array.FindIndex(db.entries, e => e.id == value);
            string[] names = db.entries.Select(e => e.name).ToArray();

            // GUI ���� ������ Ȯ���ϱ� ���� �� �ڵ� ����� �����Ѵ�.
            // �ڵ� ��� ���̿��� ���� �������� �Ͼ ��� End�Լ��� true�� ��ȯ�Ѵ�.
            EditorGUI.BeginChangeCheck();

            int selected = EditorGUI.Popup(position, "Ammo", index, names);

            if(EditorGUI.EndChangeCheck())  // �ڵ� ����� �����ϰ� GUI ���� ������ Ȯ��.
            {
                property.intValue = db.entries[selected].id;
            }
        }
    }
}