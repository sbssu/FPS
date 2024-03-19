using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

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

    [System.Serializable]
    class TrailConfig
    {
        public Material material;
        public AnimationCurve widthCurve;
        public float duration = 0.5f;
        public float minVertexDistance = 0.1f;
        public Gradient color;

        public float missDistance = 100f;
        public float simulationSpeed = 100f;
    }

    [Header("State")]
    [SerializeField] STATE state;
    [SerializeField] TRIGGER trigger;

    [Ammo]
    [SerializeField] int ammoType;
    [SerializeField] bool isAutoReload;

    [Header("Object")]
    [SerializeField] Bullet bulletPrefab;   // �Ѿ� ������.
    [SerializeField] Transform muzzle;      // �ѱ�.
    [SerializeField] TrailConfig config;   

    [Header("Animator Clip")]
    [SerializeField] AnimationClip takeOutClip;
    [SerializeField] AnimationClip fireClip;
    [SerializeField] AnimationClip reloadClip;

    [Header("Parametor")]
    [SerializeField] float fireRate;        // ����ӵ�.
    [SerializeField] float speed;           // ź��.
    [SerializeField] float damage;          // ������.
    [SerializeField] int maxAmmo;           // �ִ� ź���.

    const float MAX_DISTANCE = 100f;

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
    ObjectPool<Bullet> pool;

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

        // ���� ��ǥ ���� ����.
        Transform camPivot = Camera.main.transform;
        hitPoint = Vector3.zero;
        if (Physics.Raycast(camPivot.position, camPivot.forward, out RaycastHit hit, MAX_DISTANCE))
            hitPoint = hit.point;
        else
            hitPoint = camPivot.position + camPivot.forward * MAX_DISTANCE;

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

    Vector3 hitPoint;
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
    public void Reload(bool isForce = false)
    {
        if (state == STATE.IDLE || isForce)
        {
            anim.SetTrigger("onReload");
        }
    }
    private IEnumerator PlayTrail(Vector3 start, Vector3 end, RaycastHit hit)
    {
        TrailRenderer instance = CreateTrail();
        instance.gameObject.SetActive(true);
        instance.transform.position = start;
        yield return null;

        instance.emitting = true;

        float distance = Vector3.Distance(start, end);
        float remaining = distance;
        while(remaining > 0)
        {
            instance.transform.position = Vector3.Lerp(
                start,
                end,
                Mathf.Clamp01(1 - (remaining / distance))
                );
            remaining -= config.simulationSpeed * Time.deltaTime;
        }

        yield return new WaitForSeconds(config.duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        Destroy(instance.gameObject);
    }
    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = config.color;
        trail.material = config.material;
        trail.widthCurve = config.widthCurve;
        trail.time = config.duration;
        trail.minVertexDistance = config.minVertexDistance;
        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return trail;
    }

    private void PlayFootstep()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(muzzle.position, hitPoint);
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