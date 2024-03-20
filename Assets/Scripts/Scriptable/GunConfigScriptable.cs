using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun Config", menuName = "Guns/Gun Config")]
public class GunConfigScriptable : ScriptableObject
{
    public LayerMask hitMask;                               // 피격 레이어.
    public Vector3 spread = new Vector3(.1f, .1f, .1f);     // 확산 범위 (=오차)
    public float fireRate = 0.25f;                          // 연사 속도
}
