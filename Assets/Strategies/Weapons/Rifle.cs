using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Gun
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
