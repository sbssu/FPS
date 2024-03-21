using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Pill : Gun
{
    bool wasTriggerDown;    // ���� Ʈ���� �ٿ� ����.

    private void LateUpdate()
    {
        wasTriggerDown = isTriggerDown;
    }
    protected override void UpdateWeaponControl()
    {
        // ���콺�� �����ٰ� ������ ����.
        if(state != STATE.TAKEOUT && wasTriggerDown && !isTriggerDown && currentAmmo > 0)
        {
            anim.SetTrigger("onFire");
            rateTimer = fireRate;
            currentAmmo -= 1;
            UIManager.Instance.UpdateAmmo(currentAmmo, owner.GetMaxAmmo(ammoType));
        }
    }

    public override void Reload(bool isForce = false)
    {
        
    }

    private void Launch()
    {
        Bullet bullet = pool.Get();                         // Ǯ������ �ϳ��� ������.
        bullet.transform.position = muzzle.position;        // �ѱ� ��ġ�� �̵�.
        bullet.transform.LookAt(hitPoint);                  // �ش� �������� �ٶ󺸴� ȸ��
        bullet.Shoot(speed, damage);                        // �߻� �Լ� ȣ��.
    }

}
