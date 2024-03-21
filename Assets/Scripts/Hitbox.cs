using UnityEngine;
using UnityEngine.Events;

public enum HITTYPE
{
    HEAD,
    UPPER,
    BOTTOM,
    ARM,
}
public class Hitbox : MonoBehaviour
{
    [SerializeField] HITTYPE type;
    [SerializeField] UnityEvent<HITTYPE, float> onHit;

    public void Hit(float damage)
    {
        onHit?.Invoke(type, damage);
    }

    public static float CalculateDamage(HITTYPE type, float damage)
    {
        // 머리는 2.0배
        // 상체는 1.0배
        // 하체는 0.7배
        // 손은 0.5배
        return type switch
        {
            HITTYPE.HEAD => damage * 2.0f,
            HITTYPE.UPPER => damage * 1.0f,
            HITTYPE.BOTTOM => damage * 0.7f,
            HITTYPE.ARM => damage * 0.5f,
            _ => damage
        };
    }
}
