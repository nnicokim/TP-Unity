using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class SecondBossZombie : BossZombie
{
    protected override void Start()
    {
        base.Start();
        ChaseState = new SecondBossStateChase(this, _walkAnimationName, _idleClips, _StateMachine);
        DashState = new SecondBossStateDash(this, _dashAnimationName, _dashClips, _StateMachine);
        AttackDashState = new SecondBossStateDashAttack(this, _attackAnimationName, _attackClips, _StateMachine);
    }

}
