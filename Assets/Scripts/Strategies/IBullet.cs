using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBullet
{
    IGun Owner { get; }
    int Damage { get; }
    float Speed { get; }
    float LifeTime { get; }

    void Travel();
    void OnCollisionEnter(Collision collision);
}
