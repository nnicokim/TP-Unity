using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBullet
{
    // Sonido
    // Prefab

    float Speed { get; }
    float LifeTime { get; }
    Gun Owner { get; }

    void Travel();
    void SetOwner(Gun owner);
}
