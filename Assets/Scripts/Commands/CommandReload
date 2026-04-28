using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdMovement : ICommand
{
    private IMoveable _moveable;
    private Vector3 _direction;
    //private float _speed;

    public CmdMovement(IMoveable moveable, Vector3 direction)
    {
        //_speed = speed;
        _moveable = moveable;
        _direction = direction;
    }

    public void Execute() => _moveable.Move(_direction);
}
