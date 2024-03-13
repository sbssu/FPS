using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[AddComponentMenu("Utility/Dummy")]
[ExecuteInEditMode]
public class Dummy : MonoBehaviour
{
    public int value = 3;

    [ContextMenuItem("파워값 계산", "CalculatePower")]
    public int power = 1;

    [Range(1, 20)]
    public int test = 4;
    public int scale = 1;

    [Multiline(4)]
    public string name = "AA";

    [Range(1f, 5f)]
    public float range;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(value);
    }

    private void Update()
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void CalculatePower()
    {
        power = 100 + 400;
    }

    [ContextMenu("Reset Property")]
    public void ResetProperty()
    {
        value = 0;
        power = 0;
        test = 0;
        scale = 1;
        name = string.Empty;
    }
}
