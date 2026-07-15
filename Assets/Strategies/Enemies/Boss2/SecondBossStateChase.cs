using Unity.VisualScripting;
using UnityEngine;

public class SecondBossStateChase : StateMachineState
{
    private new readonly BossZombie enemy;
    public SecondBossStateChase(BossZombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        this.enemy = enemy;
        limitTime = -1;
        oneshotAnimation=false;
        oneshotAudioclip=false;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        float random = Random.Range(0f,1f);

        if (!enemy.IsTargetInChaseRange() && elapsedTime >= cooldownTime) 
            stateMachine.ChangeState(enemy.IdleState);
        else if (enemy.IsTargetInDashAttackRange() && elapsedTime >= cooldownTime && random < 0.01)
            stateMachine.ChangeState(enemy.AttackDashState);
        else if (enemy.IsTargetInAttackRange() && elapsedTime >= cooldownTime)
        {
            if (random > 0.7 && enemy.IsSafeDashDistance()) //TODO modularize;
                stateMachine.ChangeState(enemy.DashState);
            else
                stateMachine.ChangeState(enemy.AttackState);
        }
        else enemy.ChaseTarget();
    }

}