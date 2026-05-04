using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Gun
{
    public override void Attack()
    {
        if (!CanShoot)
            return;

        int bulletsToShoot = Mathf.Min(BulletsPerShot, _bulletCount);

        Quaternion shootRotation = transform.parent != null ? transform.parent.rotation : transform.rotation;
        Vector3 shootDirection = transform.parent != null ? transform.parent.forward : transform.forward;

        for (int i = 0; i < bulletsToShoot; i++)
        {
            Vector3 spawnPosition = transform.position + shootDirection * i * 0.8f;

            GameObject bullet = Instantiate(
                BulletPrefab,
                spawnPosition,
                shootRotation,
                ParentTransform
            );

            _bulletCount--;

            IBullet bulletBehaviour = bullet.GetComponent<IBullet>();

            if (bulletBehaviour == null)
            {
                Debug.LogError($"El prefab {BulletPrefab.name} no tiene un componente IBullet!!!", bullet);
                Destroy(bullet);
                return;
            }

            bulletBehaviour.SetOwner(this);
            bullet.name = "Bullet";
        }

        base.Attack();
    }
}
