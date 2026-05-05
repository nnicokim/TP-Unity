using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : Actor, IMovable
{
    // Velocidad para girar -> de desplazamiento
    public float Speed => Stats.MoveSpeed;

    public void Move(Vector3 direction)
        => transform.Translate(direction * Speed * Time.deltaTime, Space.World);
    //transform.Translate(0f, 0f, direction.z * Speed * Time.deltaTime);        
}
