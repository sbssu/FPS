using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Impact System/Spawn Object Effect", fileName = "SpawnObjectEffect")]
[System.Serializable]
public class SpawnObjectEffect : ScriptableObject
{
    public GameObject prefab;
    public float probability = 1;
    public bool randomizeRotation;
    public Vector3 randomizeRotationMultiplier = Vector3.zero;

}
