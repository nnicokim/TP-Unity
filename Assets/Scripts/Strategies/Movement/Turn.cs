using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : Actor, IMoveable
{
    // Velocidad para girar -> de rotacion
    public float Speed => Stats.RotationSpeed;

    public void Move(Vector3 direction)
        => transform.Rotate(direction, Speed * Time.deltaTime);
    //=> transform.Rotate(0f, direction.y * Speed * Time.deltaTime, 0f);
}
