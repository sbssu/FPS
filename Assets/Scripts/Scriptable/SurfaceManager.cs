using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SurfaceManager : MonoBehaviour
{
        private class TextureAlpha
    {
        public Texture texture;
        public float alpha;
    }

    public static SurfaceManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] List<SurfaceType> surfaces = new List<SurfaceType>();
    [SerializeField] int defaultPoolSize = 10;
    [SerializeField] Surface defaultSurface;

    public void HandleImpact(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal, ImpactType impact)
    {
        if(hitObject.TryGetComponent<Terrain>(out Terrain terrain))        
        {
            List<TextureAlpha> activeTextures = GetActiveTextureFromTerrain(terrain, hitPoint);         // �ͷ��� ���ο��� Ȱ��ȭ �ؽ�ó ����Ʈ ��������.
            foreach(TextureAlpha activeTexture in activeTextures)
            {
                SurfaceType surfaceType = surfaces.Find(surface => surface.albedo == activeTexture.texture);    // 
                if(surfaceType != null)
                {
                    foreach(Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.surface.impactTypeEffects)
                    {
                        if(typeEffect.impactType == impact)
                        {
                            // ����Ʈ ���.
                            PlayEffect(hitPoint, hitNormal, typeEffect.surfaceEffect, 1f);
                        }    
                    }
                }
                else
                {
                    foreach(Surface.SurfaceImpactTypeEffect typeEffect in defaultSurface.impactTypeEffects)
                    {
                        if(typeEffect.impactType == impact)
                        {
                            // �⺻ ���.
                            PlayEffect(hitPoint, hitNormal, typeEffect.surfaceEffect, 1f);
                        }
                    }
                }
            }
        }
    }

    
    private List<TextureAlpha> GetActiveTextureFromTerrain(Terrain terrain, Vector3 hitPoint)
    {
        // �ͷ��� ���ο��� �ǰݵ� ��ġ�� ���
        Vector3 terrainPosition = hitPoint - terrain.transform.position;
        Vector3 splatMapPosition = new Vector3(
            terrainPosition.x / terrain.terrainData.size.x,
            0f,
            terrainPosition.z / terrain.terrainData.size.z);

        int x = Mathf.FloorToInt(splatMapPosition.x * terrain.terrainData.alphamapWidth);
        int z = Mathf.FloorToInt(splatMapPosition.x * terrain.terrainData.heightmapResolution);

        // Alpha�� ��������.
        float[,,] alphaMap = terrain.terrainData.GetAlphamaps(x, z, 1, 1);

        // Ȱ��ȭ Alpha�� ����.
        List<TextureAlpha> activeTextures = new List<TextureAlpha>();
        for(int i = 0; i<alphaMap.Length; i++)
        {
            if (alphaMap[0,0,i] > 0)
            {
                activeTextures.Add(new TextureAlpha()
                {
                    texture = terrain.terrainData.terrainLayers[i].diffuseTexture,
                    alpha = alphaMap[0, 0, i]
                });
            }
        }
        return activeTextures;
    }
    private Texture GetActiveTextureFromRenderer(Renderer renderer, int triangleIndex)
    {
        if(renderer.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
        {
            Mesh mesh = meshFilter.mesh;
            if (mesh.subMeshCount > 1)
            {
                int[] hitTriangleIndices = new int[]
                {
                    mesh.triangles[triangleIndex * 3],
                    mesh.triangles[triangleIndex * 3 + 1],
                    mesh.triangles[triangleIndex * 3 + 2]
                };

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    int[] submeshTriangles = mesh.GetTriangles(i);
                    for (int j = 0; j < submeshTriangles.Length; j += 3)
                    {
                        if (submeshTriangles[j] == hitTriangleIndices[0]
                            && submeshTriangles[j + 1] == hitTriangleIndices[1]
                            && submeshTriangles[j + 2] == hitTriangleIndices[2])
                        {
                            return renderer.sharedMaterials[i].mainTexture;
                        }
                    }
                }
            }
            else
                return renderer.sharedMaterial.mainTexture;
        }

        return null;
    }

    private void PlayEffect(Vector3 hitPoint, Vector3 hitNormal, SurfaceEffect surfaceEffect, float soundOffset)
    {
        foreach(SpawnObjectEffect spawnObjectEffect in surfaceEffect.spawnObjectEffects)    // surface���ο� �����ϴ� ��� ����Ʈ ��ȸ
        {
            if(spawnObjectEffect.probability > Random.value)    // Ȯ�� üũ.
            {
                GameObject instance = Instantiate(spawnObjectEffect.prefab);        // ������ ����.
                instance.transform.forward = hitNormal;                             // ��ְ��� ���� ȸ��
                if(spawnObjectEffect.randomizeRotation)                             // ���� ȸ�� �ɼ��� Ȱ��ȭ�� ���
                {
                    Vector3 offset = new Vector3(
                        Random.Range(0, 180 * spawnObjectEffect.randomizeRotationMultiplier.x),
                        Random.Range(0, 180 * spawnObjectEffect.randomizeRotationMultiplier.y),
                        Random.Range(0, 180 * spawnObjectEffect.randomizeRotationMultiplier.z)
                        );
                    instance.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + offset);    // ���� ȸ���� + ���� ȸ���� ����
                }
            }
        }

        foreach(PlayAudioEffect playAudioEffect in surfaceEffect.playAudioEffects)
        {
            AudioClip clip = playAudioEffect.audioClips[Random.Range(0, playAudioEffect.audioClips.Count)];     // ������ ����� Ŭ�� ����
            AudioSource audioSource = Instantiate(playAudioEffect.audioSourcePrefab);                           // ����� �ҽ� �ν��Ͻ�
            audioSource.transform.position = hitPoint;                                                          // ��ġ �� ����.
            audioSource.PlayOneShot(clip, soundOffset * Random.Range(playAudioEffect.volumnRange.x, playAudioEffect.volumnRange.y));    // ȿ���� ���.
            StartCoroutine(DisableAudioSource(audioSource, clip.length));                                                               // ȿ���� ���� �� ����.
        }
    }
    private IEnumerator DisableAudioSource(AudioSource audioSource, float time)
    {
        yield return null;
        yield return new WaitForSeconds(time);
        Destroy(audioSource.gameObject);
    }
    
}
