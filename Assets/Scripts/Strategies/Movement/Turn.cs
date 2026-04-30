using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : Actor, IMoveable
{
    public float Speed => Stats.RotationSpeed;

    public void Move(Vector3 direction)
        => transform.Rotate(direction, Speed * Time.deltaTime);
    //=> transform.Rotate(0f, direction.y * Speed * Time.deltaTime, 0f);
}
