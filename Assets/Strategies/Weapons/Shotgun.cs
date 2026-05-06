using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    public override void Attack()
    {
        if (!CanShoot)
            return;

        CreateRandomBullets();

        _bulletCount--;
        base.Attack();
    }
}
