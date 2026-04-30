using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdMove : ICommand
{
    private IMoveable _strategy;
    private Vector3 _direction;

    public CmdMove(IMoveable strategy, Vector3 direction)
    {
        _strategy = strategy;
        _direction = direction;
    }

    public void Execute() => _strategy.Move(_direction);
}
