using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdAttack : ICommand
{
    private IGun _gun;

    public CmdAttack(IGun gun) => _gun = gun;

    public void Execute() => _gun.Attack();
}
