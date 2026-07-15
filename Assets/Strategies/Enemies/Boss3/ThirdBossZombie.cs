using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ThirdBossZombie : BossZombie
{
    protected override void Start()
    {
        base.Start();
        ChaseState = new ThirdBossStateChase(this, _walkAnimationName, _idleClips, _StateMachine);
        AttackDashState = new SecondBossStateDashAttack(this, _attackAnimationName, _attackClips, _StateMachine);
    }

}
