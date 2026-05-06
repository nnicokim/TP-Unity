using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Gun
{
    public override void Attack()
    {
        if (CanShoot)
        {
            Quaternion shootRotation = GetShootRotation(transform.position);
            CreateBullet(transform.position, shootRotation);
            _bulletCount--;
            base.Attack();
        }
    }
}
