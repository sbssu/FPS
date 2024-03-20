using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : ScriptableObject
{
    [SerializeField]
    public class SurfaceImpactTypeEffect
    {
        public ImpactType impactType;           // hit effect 종류
        public SurfaceEffect surfaceEffect;     // 질감
    }

    public List<SurfaceImpactTypeEffect> impactTypeEffects = new List<SurfaceImpactTypeEffect>();
}
