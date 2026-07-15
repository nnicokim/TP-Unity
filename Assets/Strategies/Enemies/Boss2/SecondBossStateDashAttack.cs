using Unity.VisualScripting;
using UnityEngine;

public class SecondBossStateDashAttack : StateMachineState
{
    private new readonly BossZombie enemy;
    private float delay = 0.5f;
    private bool hasDashed;
    public SecondBossStateDashAttack(BossZombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        this.enemy = enemy;
        limitTime = enemy.DashLength;
        oneshotAnimation=false;
        oneshotAudioclip=true;
        limitTime = 1.5f;
    }

    public override void Enter()
    {
        base.Enter();
        hasDashed = false;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (elapsedTime >= delay && !hasDashed)
        {
            enemy.StartDash(true);
            hasDashed = true;
        }
        if (enemy.IsTargetInAttackRange()) stateMachine.ChangeState(enemy.AttackState);
        else if (elapsedTime >= limitTime)
            if (enemy.IsTargetInChaseRange()) 
                stateMachine.ChangeState(enemy.ChaseState);
            else
                stateMachine.ChangeState(enemy.IdleState);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.StopDash();
    }
}