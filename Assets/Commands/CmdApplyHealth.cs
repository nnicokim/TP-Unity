using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdApplyHealth : ICommand
{
    private IDamageable _damageable;
    private int _health;

    public CmdApplyHealth(IDamageable damageable, int health)
    {
        _damageable = damageable;
        _health = health;
    }

    public void Execute() => _damageable.ApplyHealthRecovery(_health);
}
