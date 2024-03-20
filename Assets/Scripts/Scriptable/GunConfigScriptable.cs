using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun Config", menuName = "Guns/Gun Config")]
public class GunConfigScriptable : ScriptableObject
{
    public LayerMask hitMask;                               // �ǰ� ���̾�.
    public Vector3 spread = new Vector3(.1f, .1f, .1f);     // Ȯ�� ���� (=����)
    public float fireRate = 0.25f;                          // ���� �ӵ�
}
