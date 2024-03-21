using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Impact System/Surface", fileName = "Surface")]
public class Surface : ScriptableObject
{
    [System.Serializable]
    public class SurfaceImpactTypeEffect
    {
        public ImpactType impactType;           // hit effect 종류
        public SurfaceEffect surfaceEffect;     // 질감
    }

    public List<SurfaceImpactTypeEffect> impactTypeEffects = new List<SurfaceImpactTypeEffect>();
}
