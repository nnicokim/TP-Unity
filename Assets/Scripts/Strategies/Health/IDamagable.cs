using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    float CurrentLife { get; }
    float MaxLife { get; }
    void TakeDamage(int damage);
}
