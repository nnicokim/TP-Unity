using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Gun
{
    public override void Attack()
    {
        if (CanShoot)
        {
            Quaternion shootRotation = transform.parent != null ? transform.parent.rotation : transform.rotation;
            GameObject bullet = Instantiate(BulletPrefab, transform.position, shootRotation, ParentTransform);
            _bulletCount--;
            base.Attack();

            IBullet bulletBehaviour = bullet.GetComponent<IBullet>();
            if (bulletBehaviour == null)
            {
                Debug.LogError($"El prefab {BulletPrefab.name} no tiene un componente IBullet.", bullet);
                Destroy(bullet);
                return;
            }

            bulletBehaviour.SetOwner(this);
            bullet.name = "Bullet";
        }
    }
}
