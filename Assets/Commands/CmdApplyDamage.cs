using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdApplyDamage : ICommand
{
    private IDamageable _damageable;
    private int _damage;

    public CmdApplyDamage(IDamageable damageable, int damage)
    {
        _damageable = damageable;
        _damage = damage;
    }

    public void Execute() => _damageable.ApplyDamage(_damage);
}
