using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : ScriptableObject
{
    [SerializeField]
    public class SurfaceImpactTypeEffect
    {
        public ImpactType impactType;           // hit effect ����
        public SurfaceEffect surfaceEffect;     // ����
    }

    public List<SurfaceImpactTypeEffect> impactTypeEffects = new List<SurfaceImpactTypeEffect>();
}
