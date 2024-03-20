using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceManager : MonoBehaviour
{
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
            
        }
    }

    /*
    private List<Texture> GetActiveTextureFromTerrain(Terrain terrain, Vector3 hitPoint)
    {
        Vector3 terrainPosition = hitPoint - terrain.transform.position;
        Vector3 splatMapPosition = new Vector3(
            terrainPosition.x / terrain.terrainData.size.x,
            0f,
            terrainPosition.z / terrain.terrainData.size.z);

        int x = Mathf.FloorToInt(splatMapPosition.x * terrain.terrainData.alphamapWidth);
        int z = Mathf.FloorToInt(splatMapPosition.x * terrain.terrainData.heightmapResolution);
    }
    */
}
