using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.FindObjectOfType<UIManager>();
            return instance;
        }
    }

    [SerializeField] Text gunNameText;
    [SerializeField] Text gunAmmoText;

    public void UpdateGunName(string name)
    {
        gunNameText.text = name;
    }
    public void UpdateAmmo(int current, int max)
    {
        gunAmmoText.text = $"{current}/{max}";
    }
}
