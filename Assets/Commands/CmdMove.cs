using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdMove : ICommand
{
    // 1. Seccion de propiedades
    private IMovable _strategy;
    private Vector3 _direction;

    // 2. Seccion de construction
    public CmdMove(IMovable strategy, Vector3 direction)
    {
        _strategy = strategy;
        _direction = direction;
    }

    // 3. Seccion de Ejecucion
    public void Execute() => _strategy.Move(_direction);
}
