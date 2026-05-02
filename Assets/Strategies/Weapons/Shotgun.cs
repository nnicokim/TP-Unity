using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    protected override float ReloadDuration => 2.5f;

    public override void Attack()
    {
        if (!CanShoot)
            return;

        int bulletsToShoot = Mathf.Min(BulletsPerShot, _bulletCount);
        Quaternion shootRotation = transform.parent != null ? transform.parent.rotation : transform.rotation;

        for (int i = 0; i < bulletsToShoot; i++)
        {
            GameObject bullet = Instantiate(BulletPrefab, transform.position + Random.insideUnitSphere * 0.4f, shootRotation, ParentTransform);

            bullet.GetComponent<IBullet>().SetOwner(this);
            bullet.name = "Bullet";
            _bulletCount--;
        }

        base.Attack();
    }
}
