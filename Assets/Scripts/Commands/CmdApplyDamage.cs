using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdApplyDamage : ICommand
{
    private IDamagable _damageable;
    private int _damage;

    public CmdApplyDamage(IDamagable damagable, int damage)
    {
        _damageable = damagable;
        _damage = damage;
    }

    public void Execute() => _damageable.TakeDamage(_damage);
}
