using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    int Life { get; }
    int MaxLife { get; }
    void ApplyDamage(int damage);
    void ApplyHealthRecovery(int amount);
    void Die();
}
