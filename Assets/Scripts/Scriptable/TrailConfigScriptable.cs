using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Trail Config")]
public class TrailConfigScriptable : ScriptableObject
{
    public Material material;                   // 질감.
    public AnimationCurve widthCurve;           // 너비 곡선.
    public float duration = 0.5f;               // 지속 시간.
    public float minVertexDistance = 0.1f;      // 최소 거리.
    public Gradient color;                      // 색상.

    public float missDistance;                  // 최대 거리.
    public float simulationSpeed;               // 속도.
}
