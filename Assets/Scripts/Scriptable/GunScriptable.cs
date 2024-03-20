using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun")]
public class GunScriptable : ScriptableObject
{
    [Header("Weapon")]
    public int id;                      // 총기 ID
    public string gunName;              // 총기 이름
    public GameObject modelPrefab;      // 프리팹
    public Vector3 spawnPoint;          // 생성 위치
    public Vector3 spawnRotate;         // 생성 회전값

    [Header("Configure")]
    public GunConfigScriptable gunConfig;       // 총기 정보.
    public TrailConfigScriptable trailConfig;   // 탄 정보.

    [Header("Parametor")]
    public MonoBehaviour activeOwner;           // 소유자(=활성)
    public float lastShootTime;                 // 최종 발사 시간
    public ParticleSystem gunSysyem;            // 총구 화염 이펙트, 총구, 방향
    public GameObject model;                    // 실제 생성된 모델
    public PoolSystem trailPool;                // 풀링
    public TrailRenderer trailPrefab;           // trail 프리팹


    public void Spawn(Transform parent, MonoBehaviour activeOwner)
    {
        // trail 프리팹 생성.
        if (trailPrefab != null)
            Destroy(trailPrefab);
        trailPrefab = CreateTrail();

        // 풀링 시스템 생성.
        PoolSystem.Create();
        PoolSystem.Instance.InitPool(trailPrefab, 30);

        // 기본 값 대입
        this.activeOwner = activeOwner;
        lastShootTime = 0f;

        // 총기 모델링 생성.
        model = Instantiate(modelPrefab);
        model.transform.SetParent(parent, false);
        model.transform.localPosition = spawnPoint;
        model.transform.localRotation = Quaternion.Euler(spawnRotate);

        gunSysyem = model.GetComponentInChildren<ParticleSystem>();
    }
    public void Shoot()
    {
        if (Time.time > gunConfig.fireRate + lastShootTime)
        {
            gunSysyem.Play();
            Vector3 shootDirection = gunSysyem.transform.forward + new Vector3(
                Random.Range(-gunConfig.spread.x, gunConfig.spread.x),
                Random.Range(-gunConfig.spread.y, gunConfig.spread.y),
                Random.Range(-gunConfig.spread.z, gunConfig.spread.z));
            shootDirection.Normalize();
            if (Physics.Raycast(gunSysyem.transform.position, shootDirection, out RaycastHit hit, float.MaxValue, gunConfig.hitMask))
            {
                activeOwner.StartCoroutine(PlayTrail(gunSysyem.transform.position, hit.point, hit));
            }
            else
            {
                
                activeOwner.StartCoroutine(PlayTrail(
                    gunSysyem.transform.position,
                    gunSysyem.transform.position + (shootDirection * trailConfig.missDistance),
                    new RaycastHit()));
            }

            lastShootTime = Time.time;
        }
    }

    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        TrailRenderer instance = trailPool.GetInstance<TrailRenderer>(trailPrefab);
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;
        yield return null;                                          // 총알이 날아간 뒤에 그려져야하기 때문에 1프레임 딜레이.

        instance.emitting = true;

        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;
        while(remainingDistance > 0f)
        {
            // Lerp를 이용해 시간 대비 현재 있어야할 위치로 이동.
            instance.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= trailConfig.simulationSpeed * Time.deltaTime;
            yield return null;
        }
        instance.transform.position = endPoint;                     // 확실하게 endPoint로 이동.
        if(hit.collider != null)
        {
            // 탄이 도착한 지점에 이펙트 추가.
        }


        yield return new WaitForSeconds(trailConfig.duration);      // 지속시간 만큼 대기 (=꼬리 부분이 끝까지 오기를 기다린다)
        yield return null;                                          // 1프레임 대기.

        instance.emitting = false;                                  // 이미션 끄기.
        PoolSystem.Instance.ReleasePool(trailPrefab, instance);     // 풀링 시스템으로 되돌리기.
    }

    private TrailRenderer CreateTrail()
    {
        // 오브젝트 생성 후 TrailRenderer 컴포넌트를 추가한다.
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();

        // 이후 config의 값을 대입한다.
        trail.colorGradient = trailConfig.color;
        trail.material = trailConfig.material;
        trail.widthCurve = trailConfig.widthCurve;
        trail.time = trailConfig.duration;
        trail.minVertexDistance = trailConfig.minVertexDistance;

        // 라이팅 효과
        trail.emitting = false;                                                 // 발광.
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;  // 그림자 끄기.

        return trail;
    }
}
