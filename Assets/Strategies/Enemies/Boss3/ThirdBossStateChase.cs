using UnityEngine;

public class ThirdBossStateChase : StateMachineState
{
    private new readonly BossZombie enemy;
    public ThirdBossStateChase(BossZombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        this.enemy = enemy;
        limitTime = 7;
        oneshotAnimation=false;
        oneshotAudioclip=false;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        float random = Random.Range(0f,1f);

        if (!enemy.IsTargetInChaseRange() && elapsedTime >= cooldownTime) 
            stateMachine.ChangeState(enemy.IdleState);
        else if (enemy.IsTargetInDashAttackRange() && elapsedTime >= cooldownTime && random < 0.05)
            stateMachine.ChangeState(enemy.AttackDashState);
        else if (elapsedTime >= limitTime && random > 0.9)
            stateMachine.ChangeState(enemy.IdleState);
        else if (enemy.IsTargetInAttackRange() && elapsedTime >= cooldownTime)
            stateMachine.ChangeState(enemy.AttackState);
        else enemy.ChaseTarget();
    }

}