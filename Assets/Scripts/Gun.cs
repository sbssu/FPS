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
        TAKEOUT,    // 무기뽑기
        IDLE,       // 대기상태
        FIRE,       // 사격
        REROAD,     // 재장전
    }
    enum TRIGGER
    {
        AUTO,       // 자동(연사)
        MANUAL,     // 수동(단발)
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
    [SerializeField] Bullet bulletPrefab;   // 총알 프리팹.
    [SerializeField] Transform muzzle;      // 총구.
    [SerializeField] TrailConfig config;   

    [Header("Animator Clip")]
    [SerializeField] AnimationClip takeOutClip;
    [SerializeField] AnimationClip fireClip;
    [SerializeField] AnimationClip reloadClip;

    [Header("Parametor")]
    [SerializeField] float fireRate;        // 연사속도.
    [SerializeField] float speed;           // 탄속.
    [SerializeField] float damage;          // 데미지.
    [SerializeField] int maxAmmo;           // 최대 탄약수.

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

    int currentAmmo;            // 현재 탄약수.
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

        // 최초에 풀링을 이용해 총알을 많이 생성한다.
        pool = new ObjectPool<Bullet>();
        pool.Setup(gameObject, bulletPrefab, 30);
    }
    public void Pickup()
    {
        if(currentAmmo <= 0)
            currentAmmo += owner.GetAmmo(ammoType, maxAmmo);

        gameObject.SetActive(true);

        anim.SetFloat("fireSpeed", fireClip.length / fireRate);     // 연사속도에 따른 애니메이션 스피드 조절.
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

        // 총의 목표 지점 갱신.
        Transform camPivot = Camera.main.transform;
        hitPoint = Vector3.zero;
        if (Physics.Raycast(camPivot.position, camPivot.forward, out RaycastHit hit, MAX_DISTANCE))
            hitPoint = hit.point;
        else
            hitPoint = camPivot.position + camPivot.forward * MAX_DISTANCE;

        // 트리거가 눌려서 총기를 발사한다.
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

        // 변수 값 갱신.
        rateTimer = Mathf.Clamp(rateTimer - Time.deltaTime, 0.0f, fireRate);

        // 파라미터 갱신.
        anim.SetFloat("movement", owner.currentSpeed);
    }

    int takeOutHash;
    int fireHash;
    int reloadHash;
    private void UpdateWeaponState()
    {
        STATE wasState = state;

        // 현재 재생중인 애니메이션 클립의 hash에 따라 총기의 상태 갱신.
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.shortNameHash == fireHash)
            state = STATE.FIRE;
        else if (info.shortNameHash == reloadHash)
            state = STATE.REROAD;
        else if (info.shortNameHash == takeOutHash)
            state = STATE.TAKEOUT;
        else
            state = STATE.IDLE;

        // 상태 변화에 따른 처리.
        if (wasState != state)
        {
            // 바로 이전 상태가 Reload였다. (=재장전이 끝났다)
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
            Bullet bullet = pool.Get();                         // 풀링에서 하나를 꺼낸다.
            bullet.transform.position = muzzle.position;        // 총구 위치로 이동.
            bullet.transform.LookAt(hitPoint);                  // 해당 지점으로 바라보는 회전
            bullet.Shoot(speed, damage);                        // 발사 함수 호출.

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


// Ammo라는 이름의 Attribute(속성)을 선언
public class AmmoAttribute : PropertyAttribute
{

}

// AmmoAttribute 프로퍼티를 대상으로 GUI를 커스터마이징하는 클래스
[CustomPropertyDrawer(typeof(AmmoAttribute))]
public class AmmoDrawer : PropertyDrawer
{
    // position : inspector상의 위치 값
    // property : 모든 유니티 property를 찾고 담을 수 있는 최상위 객체
    // label : gui상의 옵션 값
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AmmoData db = Database.Instance.ammoData;
        if (db.entries == null || db.entries.Length == 0)
        {
            EditorGUI.HelpBox(position, "DB에 탄약 정보가 기입되지 않았습니다.", MessageType.Warning);
        }
        else
        {
            int value = property.intValue;
            int index = System.Array.FindIndex(db.entries, e => e.id == value);
            string[] names = db.entries.Select(e => e.name).ToArray();

            // GUI 변경 사항을 확인하기 위해 새 코드 블록을 시작한다.
            // 코드 블록 사이에서 무언가 변경점이 일어난 경우 End함수가 true를 반환한다.
            EditorGUI.BeginChangeCheck();

            int selected = EditorGUI.Popup(position, "Ammo", index, names);

            if(EditorGUI.EndChangeCheck())  // 코드 블록을 종료하고 GUI 변경 사항을 확인.
            {
                property.intValue = db.entries[selected].id;
            }
        }
    }
}