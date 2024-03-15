using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int[] array = new int[3] {1,2,3 };
        ArrayUtility.Add(ref array, 20);
        Debug.Log(string.Join(',', array));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
