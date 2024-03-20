using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun")]
public class GunScriptable : ScriptableObject
{
    [Header("Weapon")]
    public int id;                      // �ѱ� ID
    public string gunName;              // �ѱ� �̸�
    public GameObject modelPrefab;      // ������
    public Vector3 spawnPoint;          // ���� ��ġ
    public Vector3 spawnRotate;         // ���� ȸ����

    [Header("Configure")]
    public GunConfigScriptable gunConfig;       // �ѱ� ����.
    public TrailConfigScriptable trailConfig;   // ź ����.

    [Header("Parametor")]
    public MonoBehaviour activeOwner;           // ������(=Ȱ��)
    public float lastShootTime;                 // ���� �߻� �ð�
    public ParticleSystem gunSysyem;            // �ѱ� ȭ�� ����Ʈ, �ѱ�, ����
    public GameObject model;                    // ���� ������ ��
    public PoolSystem trailPool;                // Ǯ��
    public TrailRenderer trailPrefab;           // trail ������


    public void Spawn(Transform parent, MonoBehaviour activeOwner)
    {
        // trail ������ ����.
        if (trailPrefab != null)
            Destroy(trailPrefab);
        trailPrefab = CreateTrail();

        // Ǯ�� �ý��� ����.
        PoolSystem.Create();
        PoolSystem.Instance.InitPool(trailPrefab, 30);

        // �⺻ �� ����
        this.activeOwner = activeOwner;
        lastShootTime = 0f;

        // �ѱ� �𵨸� ����.
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
        yield return null;                                          // �Ѿ��� ���ư� �ڿ� �׷������ϱ� ������ 1������ ������.

        instance.emitting = true;

        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;
        while(remainingDistance > 0f)
        {
            // Lerp�� �̿��� �ð� ��� ���� �־���� ��ġ�� �̵�.
            instance.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= trailConfig.simulationSpeed * Time.deltaTime;
            yield return null;
        }
        instance.transform.position = endPoint;                     // Ȯ���ϰ� endPoint�� �̵�.
        if(hit.collider != null)
        {
            // ź�� ������ ������ ����Ʈ �߰�.
        }


        yield return new WaitForSeconds(trailConfig.duration);      // ���ӽð� ��ŭ ��� (=���� �κ��� ������ ���⸦ ��ٸ���)
        yield return null;                                          // 1������ ���.

        instance.emitting = false;                                  // �̹̼� ����.
        PoolSystem.Instance.ReleasePool(trailPrefab, instance);     // Ǯ�� �ý������� �ǵ�����.
    }

    private TrailRenderer CreateTrail()
    {
        // ������Ʈ ���� �� TrailRenderer ������Ʈ�� �߰��Ѵ�.
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();

        // ���� config�� ���� �����Ѵ�.
        trail.colorGradient = trailConfig.color;
        trail.material = trailConfig.material;
        trail.widthCurve = trailConfig.widthCurve;
        trail.time = trailConfig.duration;
        trail.minVertexDistance = trailConfig.minVertexDistance;

        // ������ ȿ��
        trail.emitting = false;                                                 // �߱�.
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;  // �׸��� ����.

        return trail;
    }
}
