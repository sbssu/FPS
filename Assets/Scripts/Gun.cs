using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


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

    [Header("State")]
    [SerializeField] STATE state;
    [SerializeField] TRIGGER trigger;

    [Ammo]
    [SerializeField] int ammoType;
    [SerializeField] bool isAutoReload;

    [Header("Object")]
    [SerializeField] Bullet bulletPrefab;   // 총알 프리팹.
    [SerializeField] Transform muzzle;      // 총구.

    [Header("Animator Clip")]
    [SerializeField] AnimationClip takeOutClip;
    [SerializeField] AnimationClip fireClip;
    [SerializeField] AnimationClip reloadClip;

    [Header("Parametor")]
    [SerializeField] float fireRate;        // 연사속도.
    [SerializeField] float speed;           // 탄속.
    [SerializeField] float damage;          // 데미지.
    [SerializeField] int maxAmmo;           // 최대 탄약수.

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
        anim.SetFloat("fireSpeed", fireClip.length / fireRate);     // 연사속도에 따른 애니메이션 스피드 조절.
        gameObject.SetActive(true);
    }
    public void PutAway()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateWeaponState();

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