using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy : IDamagable, IMoveable
{
    int Damage { get; }
    void EnemyAttackDamage(int damage);
}
