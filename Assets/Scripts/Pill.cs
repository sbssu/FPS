using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Pill : Gun
{
    bool wasTriggerDown;    // 이전 트리거 다운 상태.

    private void LateUpdate()
    {
        wasTriggerDown = isTriggerDown;
    }
    protected override void UpdateWeaponControl()
    {
        // 마우스를 눌렀다가 해제한 시점.
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
        Bullet bullet = pool.Get();                         // 풀링에서 하나를 꺼낸다.
        bullet.transform.position = muzzle.position;        // 총구 위치로 이동.
        bullet.transform.LookAt(hitPoint);                  // 해당 지점으로 바라보는 회전
        bullet.Shoot(speed, damage);                        // 발사 함수 호출.
    }

}
