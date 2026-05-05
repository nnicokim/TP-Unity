using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Run : Actor, IMovable
{
    // Velocidad para girar -> de desplazamiento
    public float Speed => Stats.RunSpeed;

    // Animacion de correr
    // Sonido de correr
    // Valor x de consumo de estamina

    public void Move(Vector3 direction)
        => transform.Translate(direction * Speed * Time.deltaTime, Space.World);
}
