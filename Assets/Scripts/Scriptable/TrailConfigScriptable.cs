using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Trail Config")]
public class TrailConfigScriptable : ScriptableObject
{
    public Material material;                   // ����.
    public AnimationCurve widthCurve;           // �ʺ� �.
    public float duration = 0.5f;               // ���� �ð�.
    public float minVertexDistance = 0.1f;      // �ּ� �Ÿ�.
    public Gradient color;                      // ����.

    public float missDistance;                  // �ִ� �Ÿ�.
    public float simulationSpeed;               // �ӵ�.
}
