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

        for (int i = 0; i < bulletsToShoot; i++)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 0.4f;
            Quaternion shootRotation = GetShootRotation(spawnPosition);
            GameObject bullet = Instantiate(BulletPrefab, spawnPosition, shootRotation, ParentTransform);

            bullet.GetComponent<IBullet>().SetOwner(this);
            bullet.name = "Bullet";
            _bulletCount--;
        }

        base.Attack();
    }
}
